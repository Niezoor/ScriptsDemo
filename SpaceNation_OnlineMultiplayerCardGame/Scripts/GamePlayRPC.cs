using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayRPC : MonoBehaviour {

	public GamePlay gPlay;
	public bool MatchStarted = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	[PunRPC]
	void SignalMatchStart()
	{
		Debug.Log("RPC received from second player");
		MatchStarted = true;
	}

	[PunRPC]
	void SignalMatchReady()
	{
		Debug.Log("RPC second player ready");
		gPlay.EnemyIsReady = true;
		gPlay.MatchStart ();
	}

	[PunRPC]
	void YouAreSecond() {
		gPlay.SetAsSecondPlayer ();
	}

	[PunRPC]
	void RPCSetEnemyHero(string name) {
		//if (MatchStarted) {
		Debug.Log("RPC received from second player, set hero: " + name);
		gPlay.SetEnemyHero (name);
		/*} else {
			waitForStart = true;
			enemyHero = name;
		}*/
	}

	[PunRPC]
	void SignalCardDraw() {
		gPlay.EnemyCardDraw ();
	}

	[PunRPC]
	void SignalInitCardDraw() {
		gPlay.EnemyInitCardDraw ();
	}

	[PunRPC]
	void RPCRemoveCardFromHand(int CardHandIdx) {
		gPlay.EnemyRemoveCardFromHand (CardHandIdx);
	}

	[PunRPC]
	void RPCPutPawnPawnOnBoard(string cardName, int pawnID, int pawnRotationIndex, int handIndex, int boardPosIndex) {
		gPlay.PutEnemyPawnOnBoard (cardName, pawnID, pawnRotationIndex, handIndex, boardPosIndex);
	}

	[PunRPC]
	void RPCChangingPlacePawnOnPos(int pawnID, int rotIndex, int index) {
		Debug.Log("RPCChangingPlacePawnOnPos pawmid: " + pawnID + "pos: " + index);
		gPlay.ChangeEnemyPawnPos (pawnID, rotIndex, index);
	}

	[PunRPC]
	void RPCConfirmPawnPos(string cardName, int pawnID, int pawnRotationIndex, int boardPosIndex) {
		gPlay.ConfirmEnemyPawnPos (cardName, pawnID, pawnRotationIndex, boardPosIndex);
	}
		
	[PunRPC]
	void RPCPutWeaponOnBoard(string weaponName, int weaponRotationIndex, int handIndex, int boardPosIndex) {
		gPlay.PutEnemyWeaponOnBoard(weaponName, weaponRotationIndex, handIndex, boardPosIndex);
	}
		
	[PunRPC]
	void RPCShowMarkForEnemy(int boardPosIndex, int handIndex) {
		gPlay.ShowEnemyMarkBeam(boardPosIndex, handIndex);
	}

	[PunRPC]
	void RPCDestroyMarkBeam() {
		gPlay.DestroyMarkBeam();
	}

	[PunRPC]
	void RPCPlayEffectCardOnBoard(string effectName, int startBoardPosIndex, int endBoardPosIndex, int handIndex) {
		gPlay.PlayEnemyEffectOnBoard(effectName, startBoardPosIndex, endBoardPosIndex, handIndex);
	}

	[PunRPC]
	void RPCGiveWeaponToPawn(string weaponName, int WeaponOwnerBoardIndex, int WeaponRotationIndex) {
		gPlay.GiveWeaponToEnemyPawn (weaponName, WeaponOwnerBoardIndex, WeaponRotationIndex);
	}

	[PunRPC]
	void RPCDoDamage(int PositionIndex, int dmgValue) {
		gPlay.DoDamageOnBoardImpl (PositionIndex, dmgValue);
	}

	[PunRPC]
	void RPCDoAttack(int myPawnPosIdx, int enemyPawnPosInx, GamePlay.attackDirections attackDirection, int onBoardDistance) {
		gPlay.DoAttackImpl (myPawnPosIdx, enemyPawnPosInx, attackDirection, onBoardDistance);
	}

	[PunRPC]
	void RPCSyncPawn(string pawnName, int pawnBoardID, int boardPosisionIndex, int boardRotationIndex , int pawnHealth, int pawnAttack) {
		gPlay.HandleBoardSync (pawnName, pawnBoardID, boardPosisionIndex, boardRotationIndex, pawnHealth, pawnAttack);
	}

	[PunRPC]
	void RPCGiveTurn() {
		gPlay.TakeTurn ();
	}
}
