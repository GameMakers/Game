using UnityEngine;
using System.Collections.Generic;

public class Shop : MonoBehaviour {

	protected string shop_name;
	//text displayed when the player accesses the store
	protected string greeting_text;





	public string to_string(){
		return shop_name;
	}

	public string get_greeting(){
		return greeting_text;
	}


}
