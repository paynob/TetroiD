using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

	int posX,posY,width, height;

	void OnGUI (){
		posX = Screen.width / 20;
		posY = Screen.height / 40;
		width = Screen.width / 2;
		height = Screen.height / 12;

		GUI.Label (new Rect (posX, posY, width, height), "Arkanoid: "+GameManager.singleton.Player2Points());
		posY += height;
		GUI.Label (new Rect (posX, posY, width, height), "Tetris: "+GameManager.singleton.Player1Points());

	}
}
