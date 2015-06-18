using UnityEngine;
using System.Collections.Generic;

public class Enemy : Character {

	//exp gained when the enemy is killed
	protected int exp_value;
	//money gained when the enemy is killed
	protected int base_money_value;
	//percentage chance that the enemy will drop an item at all
	protected int drop_percent;
	//map of items as keys and values representing the percentage chance of the item being selected as a drop
	protected Dictionary<Item, int> item_drops;

	protected List<Attack> attack_types;

	protected bool can_fly;
	protected bool can_swim;
	protected bool can_climb;
	//the enemy can see through walls
	protected bool has_xray;
	//from how far away the enemy can see a player
	protected int vision_distance;
	//from how far away the enemy can hear a player
	protected int hearing_distance;

	//inital location - UPDATE THIS TO A BETTER STRUCTURE	
	protected int x;
	protected int y;
	//how far from their initial location the enemy may venture
	protected int movement_radius;
	//overall behaviour of the enemy
	protected AI inelligence_type;
	//how likely the enemy is to engage in combat
	protected int base_aggro;
	//passive, defensive, patrolling, aggressive, etc
	protected int current_action;
	protected Playable_character target;






	public int get_exp(){
		return exp_value;
	}

	public int get_money(){
		//USE RANDOMIZATION OVER SOME RANGE TO VARY THE AMOUNT OF MONEY DROPPED
		return base_money_value;
	}

	public Item drop_item(){
		//APPLY DROP PERCENTAGE TO CHECK IF AN ITEM WILL BE DROPPED
			//IF AN ITEM IS TO BE DROPPED, SELECT ONE FROM THE LIST OF DROPS BASED ON THEIR PERCENTAGES
		return null;
	}

	public List<Attack> get_attack_types(){
		return attack_types;
	}


	//CREATE ENEMY BEHAVIOUR FUNCTIONALITY BASED ON THE RELEVANT ATTRIBUTES
	
}
