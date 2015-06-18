using UnityEngine;
using System.Collections.Generic;

public class Workshop : Shop {

	protected List<Item> upgradable_items;
	
	
	public List<Item> get_upgradable_items(){
		return upgradable_items;
	}

}
