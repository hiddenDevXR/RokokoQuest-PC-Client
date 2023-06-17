using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserElement : MonoBehaviour
{
    public Text nameText;
    public int playerIndex = 0;
    public NetworkManager networkManager;

    public void SetData(string nickName, int number)
    {
        nameText.text = "User " + nickName;
        playerIndex = number;
    }

    public void CallPlayer()
    {
        networkManager.CallNextPlayer(playerIndex);
    }
}
