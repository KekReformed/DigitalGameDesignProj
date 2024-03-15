using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public string name;
    public bool upgradeEnabled;

    public Upgrade createUpgrade(string upgradeName, int upgradeIndex) {
        name = upgradeName;
        upgradeEnabled = true;
        GetComponent<Inventory>().upgrades[upgradeIndex] = this;
        return this;
    }
}
