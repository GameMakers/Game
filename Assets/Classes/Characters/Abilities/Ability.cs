using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour {

	protected string ability_type;

	protected int ability_level;
	
	
	
	public string to_string(){
		return ability_type;
	}

	public int get_ability_level(){
		return ability_level;
	}
}
