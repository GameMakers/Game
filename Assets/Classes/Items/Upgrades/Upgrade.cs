using UnityEngine;
using System.Collections.Generic;

public class Upgrade : MonoBehaviour {

	protected string upgrade_type;

	//base amounts of materials needed to make the upgrade
	protected Dictionary<Material, int> base_materials;
	//full items required in addition to the materials to make the upgrade
	protected Dictionary<Item, int> required_items;
	//upgrades which must be made prior to this one
	protected List<Upgrade> prerequisites;




	public string to_string(){
		return upgrade_type;
	}

	public Dictionary<Material, int> get_base_materials(){
		return base_materials;
	}

	public Dictionary<Item, int> get_required_items(){
		return required_items;
	}

	public List<Upgrade> get_prerequisities(){
		return prerequisites;
	}

}
