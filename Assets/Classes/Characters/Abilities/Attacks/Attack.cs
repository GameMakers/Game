using UnityEngine;
using System.Collections.Generic;

public class Attack : Ability {
	
	protected int base_damage;
	protected int range;
	protected int angle;
	protected int width;
	protected int height;
	protected int speed;
	protected int armor_penetration;
	//how much impact the attack has on environment destruction
	protected int enironment_destructability;
	//from how far away the attack can be heard
	protected int attack_sound_distance;
	//from how far away the impact can be heard
	protected int impact_sound_distance;

	//status conditions which the attack can inflict and percentage chance of infliction
	protected Dictionary<Status_condition, int> conditions;

	//whether the attack can pass through walls
	protected bool is_rail = false;
	//whether the attack will arc or travel in a straight line (if it is a projectile)
	protected bool has_arc = false;
	//whether the attack will detonate on impact
	protected bool has_impact_detonation = false;
	//whether the attack will detonate after a period of time
	protected bool has_timed_detonation = false;
	protected int detonation_radius = 0;
	protected int detonation_damage = 0;

	protected string animation_type;


	


	public int get_damage(){
		return base_damage;
	}

	public int get_range(){
		return range;
	}

	public int get_angle(){
		return angle;
	}

	public int get_width(){
		return width;
	}

	public int get_height(){
		return height;
	}

	public int get_speed(){
		return speed;
	}

	public int get_armor_penetration(){
		return armor_penetration;
	}

	public int get_enironment_destructability(){
		return enironment_destructability;
	}

	public int get_attack_sound_distance(){
		return attack_sound_distance;
	}

	public int get_impact_sound_distance(){
		return impact_sound_distance;
	}

	public Dictionary<Status_condition, int> get_conditions(){
		return conditions;
	}

	public bool attack_is_rail(){
		return is_rail;
	}

	public bool attack_has_arc(){
		return has_arc;
	}

	public bool attack_has_impact_detonation(){
		return has_impact_detonation;
	}

	public bool attack_has_timed_detonation(){
		return has_timed_detonation;
	}

	public int get_detonation_radius(){
		return detonation_radius;
	}

	public int get_detonation_damage(){
		return detonation_damage;
	}

	public string get_animation_type(){
		return animation_type;
	}


	public List<Status_condition> get_conditions_inflicted(){
		List<Status_condition> inflicted_conditions = new List<Status_condition>();
		//DETERMINE WHICH CONDITIONS WERE INFLICTED AND RETURN A LIST OF THEM
		return inflicted_conditions;
	}

}
