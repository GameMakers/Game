using UnityEngine;
using System.Collections.Generic;

public class Character_class : MonoBehaviour {

	protected string class_type;

	protected Character_type character_type;
	protected List<Attack> attack_types;
	protected List<Skill> skill_types;

	//the base amounts at which the stats for a character of this class will begin
	protected int[] base_stats = new int[5];
	//the amount each stat will increase by upon levelling up for a character of this class
	protected int[] stat_progression = new int[5];



	
	public string to_string(){
		return class_type;
	}

	public Character_type get_character_type(){
		return character_type;
	}

	public List<Attack> get_attack_types(){
		return attack_types;
	}

	public List<Skill> get_skill_types(){
		return skill_types;
	}

	public int[] get_base_stats(){
		return base_stats;
	}

	public int[] get_stat_progression(){
		return stat_progression;
	}
}
