using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SNMovementController))]
[RequireComponent(typeof(SNCharacter))]
public class SNEnemyController : MonoBehaviour {
	private SNMovementController movementController;
	private SNCharacter character;

	[Header("Timings")]
	public float attackLoadTime;
	private float currentAttackLoadTime;
	public float attackDuration;
	public float timeBetweenAttacks;
	private float currentTimeBetweenAttacks;

	public float timeBetweenShots;
	private float currentTimeBetweenShots;
	public int shotsInSingleAttack;
	private int currentShotsInSingleAttack;

	[Header("Distance")]
	public float chaseRange;
	public float retreadRange;
	public float shotRange;
	public float meleeRange;
	public float patrolRange;

	[Header("AI")]
	public bool clearLineOfSight = false;
	public bool canShoot;
	public bool canMelee;

	[System.Serializable]
	public enum AIStates
	{
		Idle, Chase, Retread, Melee, Shooting, Wait, LookingForWayAround,
	}
	public AIStates AIState;

	[System.Serializable]
	public enum AIAggroBases
	{
		Closest, ReceivedDamage, DealedDamage, AllDamage,
	}
	[Header("Target Aggro")]
	public AIAggroBases AggroBase;

	[System.Serializable]
	public class AITarget {
		public GameObject targetObject;
		public SNCharacter targetCharacter;
		public SNCharacter myCharacter;
		public bool spotted = false;
		public bool lineOfSight = false;
		public float distance;
		public float dealedDamageToMe;
		public float dealedDamageByMe;
		public void IncreaseDealedDamage (SNCharacter target, int damage) {
			if (target.Equals (myCharacter)) {
				dealedDamageToMe += damage;
			}
		}
		public void IncreaseReceivedDamage (SNCharacter target, int damage) {
			if (target.Equals (myCharacter)) {
				dealedDamageByMe += damage;
			}
		}
	}
	[Header("Target List")]
	public List<AITarget> AITargets = new List<AITarget>();
	public AITarget target;

	public Vector2 inputMove;
	private Vector3 targetPosition;

	// Use this for initialization
	void Start () {
		movementController = GetComponent<SNMovementController> ();
		character = GetComponent<SNCharacter> ();
		AIState = AIStates.Idle;
	}

	// Update is called once per frame
	void Update () {
		if (!character.dead) {
			if (target != null && target.targetObject != null) {
				CheckSight (target);
				float targetDistance = Vector2.Distance (this.transform.position, target.targetObject.transform.position);
				target.distance = targetDistance;
				inputMove = Vector2.zero;
				if (AIState != AIStates.Melee && AIState != AIStates.Shooting && AIState != AIStates.Wait) {
					HandleMove (targetDistance, target);
				}
				if (AIState == AIStates.Melee) {
					HandleMelee ();
				}
				if (AIState == AIStates.Shooting) {
					HandleShooting ();
				}

				if (currentTimeBetweenAttacks <= 0) {
					if (target.lineOfSight) {
						if (canMelee && (targetDistance <= meleeRange)) {
							if (AIState != AIStates.Melee) {
								currentAttackLoadTime = attackLoadTime;
								targetPosition = target.targetObject.transform.position;
								movementController.lastInputMove = (targetPosition - this.transform.position).normalized;
								movementController.PlayAnimation (
									SNMovementController.AnimationsStates.Prepare, true);
								character.immune = true;
								AIState = AIStates.Melee;
							}
						} else if (canShoot && (targetDistance <= shotRange)) {
							if (AIState != AIStates.Melee) {
								currentAttackLoadTime = attackLoadTime;
								targetPosition = target.targetObject.transform.position;
								movementController.PlayAnimation (
									SNMovementController.AnimationsStates.Prepare, true);
								AIState = AIStates.Shooting;
							}
						}
					}
				} else {
					currentTimeBetweenAttacks -= Time.deltaTime;
				}

				if (target.targetCharacter.dead) {
					AITargets.Remove (target);
					target = null;
				}
			} else {
				FindTargets ();
			}
			movementController.MoveUpdate (inputMove);
			target = ChooseTarget (AggroBase);
		}
	}

	private void HandleMove(float targetDistance, AITarget target) {
		bool move = false;
		inputMove = Vector2.zero;

		if (AIState != AIStates.LookingForWayAround) {
			if (target.spotted && targetDistance >= meleeRange) {
				AIState = AIStates.Chase;
			} else if (targetDistance <= chaseRange && targetDistance >= meleeRange) {
				target.spotted = true;
				AIState = AIStates.Chase;
			} else {
				AIState = AIStates.Idle;
			}
			if (AIState == AIStates.Chase) {
				if (target.lineOfSight) {
					inputMove = target.targetObject.transform.position - this.transform.position;
				} else {
					Vector3 newPos = new Vector3 (this.transform.position.x + Random.Range (-patrolRange, patrolRange),
						                 this.transform.position.y + Random.Range (-patrolRange, patrolRange),
						                 this.transform.position.z);
					targetPosition = newPos;
					inputMove = targetPosition - this.transform.position;
					AIState = AIStates.LookingForWayAround;
					SetToIdleAfter (0.8f);
				}
				move = true;
			}
		} else {
			if (Vector2.Distance (targetPosition, this.transform.position) > 0.01) {
				inputMove = targetPosition - this.transform.position;
				move = true;
			}
		}
		if (targetDistance <= retreadRange) {
			inputMove = this.transform.position - target.targetObject.transform.position;
			AIState = AIStates.Retread;
			move = true;
		}
		if (!move) {
			AIState = AIStates.Idle;
		}
	}

	private void SetToIdleAfter(float after) {
		StartCoroutine (SetToIdleAfterTask (after));
	}

	private IEnumerator SetToIdleAfterTask(float after) {
		yield return new WaitForSeconds (after);
		movementController.UnlockAnimations ();
		AIState = AIStates.Idle;
	}

	private void CheckSight(AITarget target) {
		bool clearView = true;
		if (target != null) {
			RaycastHit2D[] hits = Physics2D.RaycastAll(
				movementController.transform.position,
				target.targetObject.transform.position - movementController.transform.position,
				Vector3.Distance(movementController.transform.position,
					target.targetObject.transform.position)
			);
			Debug.DrawRay (movementController.transform.position,
				target.targetObject.transform.position - movementController.transform.position);
			for(int i = 0; i < hits.Length; i++) {
				RaycastHit2D hit = hits[i];
				if (hit.collider.gameObject.layer != LayerMask.NameToLayer ("Dash")) {
					//Debug.Log (hits.Length + " hitted: " + hit.collider.gameObject.name);
					if (hit.collider.attachedRigidbody != null) {
						if (!GameObject.ReferenceEquals (hit.collider.attachedRigidbody.gameObject, target.targetObject)) {
							clearView = false;
						}
					}
				}
			}
		}
		target.lineOfSight = clearView;
	}

	private void HandleMelee() {
		if (AIState == AIStates.Melee) {
			if (currentAttackLoadTime <= 0) {
				character.Attack ();
				currentTimeBetweenAttacks = timeBetweenAttacks;
				AIState = AIStates.Wait;
				targetPosition = target.targetObject.transform.position;
				movementController.lastInputMove = (targetPosition - this.transform.position).normalized;
				SetToIdleAfter (attackLoadTime);
				character.immune = false;
			} else {
				currentAttackLoadTime -= Time.deltaTime;
			}
		}
	}

	private void HandleShooting() {

	}

	private AITarget ChooseTarget(AIAggroBases aggroBase) {
		AITarget rv = null;
		for (int i = 0; i < AITargets.Count; i++) {
			if (AITargets [i].targetCharacter.dead) {
				Debug.Log ("remove target");
				AITargets.Remove (AITargets [i]);
			}
		}
		for (int i = 0; i < AITargets.Count; i++) {
			if (AITargets [i].targetObject == null) {
				Debug.Log ("remove target");
				AITargets.Remove (AITargets [i]);
			}
		}

		for (int i = 0; i < AITargets.Count; i++) {
			if (Vector3.Distance (this.transform.position, AITargets [i].targetObject.transform.position) <= chaseRange) {
				AITargets [i].spotted = true;
			}
		}

		if (AITargets.Count == 1) {
			if (AITargets [0].spotted) {
				return AITargets [0];
			} else {
				return null;
			}
		} else if (AITargets.Count == 0) {
			return null;
		}
		int last = 0;
		bool found = false;
		if (AITargets [last].targetObject != null) {
			float lastDist = -1f;
			for (int i = 0; i < AITargets.Count; i++) {
				if (AITargets [i].spotted) {
					found = true;
					if (aggroBase == AIAggroBases.Closest) {
						float dist = Vector3.Distance (this.transform.position, AITargets [i].targetObject.transform.position);
						if ((lastDist > dist) || (lastDist == -1f)) {
							lastDist = dist;
							last = i;
						}
					} else if (aggroBase == AIAggroBases.AllDamage) {
						if ((AITargets [last].dealedDamageToMe + AITargets [last].dealedDamageByMe) <
							(AITargets [i].dealedDamageToMe + AITargets [i].dealedDamageToMe)) {
							last = i;
						}
					} else if (aggroBase == AIAggroBases.ReceivedDamage) {
						if (AITargets [last].dealedDamageByMe < AITargets [i].dealedDamageByMe) {
							last = i;
						}
					} else if (aggroBase == AIAggroBases.DealedDamage) {
						if (AITargets [last].dealedDamageToMe < AITargets [i].dealedDamageToMe) {
							last = i;
						}
					}
				}
			}
		}
		if (found) {
			Debug.LogWarning ("best target not found, search for closest");
			rv = AITargets [last];
			if (aggroBase == AIAggroBases.AllDamage) {
				if (AITargets [last].dealedDamageToMe + AITargets [last].dealedDamageByMe == 0) {
					rv = ChooseTarget (AIAggroBases.Closest);
				}
			} else if (aggroBase == AIAggroBases.ReceivedDamage) {
				if (AITargets [last].dealedDamageByMe == 0) {
					rv = ChooseTarget (AIAggroBases.Closest);
				}
			} else if (aggroBase == AIAggroBases.DealedDamage) {
				if (AITargets [last].dealedDamageToMe == 0) {
					rv = ChooseTarget (AIAggroBases.Closest);
				}
			}
		}
		return rv;
	}

	private void FindTargets() {
		Debug.Log ("Search targets");
		//AITargets.Clear ();//next time clear it only on new round
		GameObject[] newtargets = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < newtargets.Length; i++) {
			bool found = false;
			for (int j = 0; j < AITargets.Count; j++) {
				if (AITargets [j].targetObject.Equals (newtargets [i])) {
					found = true;
					break;
				}
			}
			if (!found) {
				Debug.Log ("add target");
				AITarget target = new AITarget ();
				target.targetCharacter = newtargets [i].transform.GetComponent<SNCharacter> ();
				if (!target.targetCharacter.dead) {
					target.targetObject = newtargets [i];
					target.myCharacter = character;
					//target.spotted = true;
					//target.targetCharacter.OnGiveDamage += target.IncreaseReceivedDamage;
					//target.targetCharacter.OnTakeDamage += target.IncreaseDealedDamage;
					AITargets.Add (target);
				}
			}
		}
	}
}
