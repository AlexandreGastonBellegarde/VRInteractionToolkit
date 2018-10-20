﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromFlexiblePointer : MonoBehaviour {
	public Material highlightMaterial;
	private Material defaultMaterial;

	public FlexiblePointer selectObject;

	// Use this for initialization
	void Start () {
		defaultMaterial = this.GetComponent<Renderer>().material;
		selectObject.hovered.AddListener(highlight);
		selectObject.unHovered.AddListener(unHighlight);	
		selectObject.selectedObject.AddListener(playSelectSound);	
	}

	void highlight() {
		if(selectObject.currentlyPointingAt == this.gameObject) {
			print("highlight");
			this.GetComponent<Renderer>().material = highlightMaterial;
		}		
	}

	void unHighlight() {
		if(selectObject.currentlyPointingAt == this.gameObject) {
			print("unhighlight");
			this.GetComponent<Renderer>().material = defaultMaterial;
		}		
	}

	void playSelectSound() {
		if(selectObject.currentlyPointingAt == this.gameObject) {
			this.GetComponent<AudioSource>().Play();
		}	
	}
}
