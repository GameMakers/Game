using UnityEngine;
using System.Collections.Generic;

public class Smith : Shop {

	protected List<Item> craftable_items;
	protected List<Item> salvageable_items;


	public List<Item> get_craftable_items(){
		return craftable_items;
	}

	public List<Item> get_salvageable_items(){
		return salvageable_items;
	}
}
