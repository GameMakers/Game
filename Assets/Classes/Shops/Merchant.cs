using UnityEngine;
using System.Collections.Generic;

public class Merchant : Shop {

	protected Dictionary<Item, int> items_for_sale;




	public Dictionary<Item, int> get_items(){
		return items_for_sale;
	}
	
	public void sell_item(int item_index){
		//SELL THE ITEM AT THE SPECIFIED INDEX TO THE CHARACTER
	}
	
	public void purchase_item(Item item){
		//PURCHASE AN ITEM FROM A CHARACTER
	}

}
