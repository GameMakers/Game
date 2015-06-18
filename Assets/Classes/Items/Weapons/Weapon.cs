using UnityEngine;
using System.Collections.Generic;

public class Weapon : Item {

	protected bool is_two_handed;
	//attacks which can be used with this weapon (character must have the attack in order to use it with the weapon)
	protected List<Attack> attack_types;
	//skills which can be used with this weapon (character must have the skill in order to use it with the weapon)
	protected List<Skill> skill_types;



	public bool is_two_handed_wep(){
		return is_two_handed;
	}

	public List<Attack> get_attack_types(){
		return attack_types;
	}
	
	public List<Skill> get_skill_types(){
		return skill_types;
	}

}
