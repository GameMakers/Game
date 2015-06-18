using UnityEngine;
using System.Collections.Generic;

public class Item : MonoBehaviour {

	protected string item_type;

	//materials needed to craft the item
	protected Dictionary<Material, int> crafting_materials;
	//base materials obtained from salvaging the item
	protected Dictionary<Material, int> salvage_materials;
	//upgrades which can be made to the item and multipliers for the material costs
	protected Dictionary<Upgrade, double> available_upgrades;



	public string to_string(){
		return item_type;
	}

	public Dictionary<Material, int> get_crafting_materials(){
		return crafting_materials;
	}
	
	public Dictionary<Material, int> get_salvage_materials(){
		return salvage_materials;
	}

	public Dictionary<Upgrade, double> get_available_upgrades(){
		return available_upgrades;
	}

}
