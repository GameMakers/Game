using UnityEngine;
using System.Collections.Generic;

public class Playable_character : Character {

	protected Character_class character_class;

	protected Weapon equiped_weapon;
	protected Offhand_equip equiped_offhand;

	protected List<Item> inventory;



	public Character_class get_character_class(){
		return character_class;
	}

	public Weapon get_weapon(){
		return equiped_weapon;
	}

	public Offhand_equip get_offhand(){
		return equiped_offhand;
	}

	public List<Item> get_inventory(){
		return inventory;
	}
}
