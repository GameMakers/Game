﻿using UnityEngine;
using System.Collections;

public class Game_object{

	private Vector3 position;
	private Vector3 scale;
	private float left_extremity;
	private float right_extremity;
	private float upper_extremity;
	private float lower_extremity;
    private int active_index;
    private bool is_object_active;


	public Game_object(){}

    public Game_object(Vector3 position, Vector3 scale) {
		this.position = position;
		this.scale = scale;
		left_extremity = position.x - (scale.x / 2);
		right_extremity = position.x + (scale.x / 2);
		upper_extremity = position.y + (scale.y / 2);
		lower_extremity = position.y - (scale.y / 2);
        is_object_active = false;
	}

	public Vector3 get_position(){
		return position;
	}

	public Vector3 getScale(){
		return scale;
	}
	
	public float get_left_extremity(){
		return left_extremity;
	}
	
	public float get_right_extremity(){
		return right_extremity;
	}
	
	public float get_upper_extremity(){
		return upper_extremity;
	}
	
	public float get_lower_extremity(){
		return lower_extremity;
	}

    public int get_index(){
        return active_index;
    }

    public bool is_active() {
        return is_object_active;
    }

	public void set_position(Vector3 position){
		this.position = position;
	}
	
	public void set_scale(Vector3 scale){
		this.scale = scale;
	}

    public void set_index(int index){
        active_index = index;
    }

    public void toggle_active() {
        is_object_active = !is_object_active;
    }

}
