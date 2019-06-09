using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UCharts;
using UnityEngine.UI;

public class RatePanel : MonoBehaviour {

	public PieChart RatePieChart;
	public Text pieChartPercentageText;
	public GameObject ColorTextPrefab;
	public Text TitleText;
	public Transform DescPanelTransform;

	// Use this for initialization
	void Start () {
		//pieChartPercentageText.text = "";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayAnim() {
		RatePieChart.PlayAnimation ();
	}

	public void SetTitle(string title) {
		if (TitleText != null) {
			TitleText.text = title;
		}
	}

	private void SetElemValue(Text Tcomp, string name, int value) {
		Tcomp.text = name + " " + value; 
	}

	public void AddRateElem(string name, int value, Color color) {
		PieChartDataNode NewNode = new PieChartDataNode ();
		GameObject desc = Instantiate (ColorTextPrefab, DescPanelTransform);
		Text descText = desc.GetComponent<Text> ();
		descText.color = color;
		SetElemValue (descText, name, value);
		NewNode.Text = name;
		NewNode.Value = value;
		RatePieChart.m_Data.Add (NewNode);
		RatePieChart.m_Colors.Add (color);
	}

	public void ModifyRateElem(string elemName, int value) {
		for (int i = 0; i < RatePieChart.m_Data.Count; i++) {
			PieChartDataNode NewNode = RatePieChart.m_Data [i];
			if (NewNode.Text.Equals (elemName)) {
				Debug.Log (" modify elem:" + elemName + " val:" + value);
				NewNode.Value = value;
				RatePieChart.m_Data [i] = NewNode;
				RatePieChart.PlayAnimation ();
				if (DescPanelTransform.childCount > i) {
					SetElemValue(DescPanelTransform.GetChild (i).GetComponent<Text>(),elemName, value);
				}
			}
		}
	}

	public void ShowPercentage() {
		if (RatePieChart.m_Data.Count == 2) {
			float var1 = RatePieChart.m_Data [0].Value;
			float var2 = RatePieChart.m_Data [1].Value;
			int rate = 0;
			if (var1 != 0) {
				rate = (int)((var1 / (var1 + var2)) * 100);
			}
			pieChartPercentageText.text = rate.ToString() + "%";
		} else {
			Debug.LogWarning ("You need to set 2 rate elements to show percentage " + RatePieChart.m_Data.Count);
			pieChartPercentageText.text = "";
		}
	}
}
