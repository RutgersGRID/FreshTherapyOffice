/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * NetworkTransform.cs Revision 1.0.1103.01
 * Simple class that is used to determine whether a transform requires an update */

using UnityEngine;
using System.Collections;

using System;

// We use this class here to store and work with transform states
public class NetworkTransform {
		
		public Vector3 position;
		public Quaternion rotation;
		private GameObject obj;

		public NetworkTransform(GameObject obj) {
			this.obj = obj;
			InitFromCurrent();
		}
				
		// Updates last state to the current transform state if the current state was changed and return true if so or false if not
		public bool UpdateIfDifferent() {
			if (obj.transform.position != this.position || obj.transform.rotation!=this.rotation) {
				InitFromCurrent();
				return true;
			}
			else {
				return false;
			}
		}	
		
		public void InitFromValues(Vector3 pos, Quaternion rot) {
			this.position = pos;
			this.rotation = rot;
		}
		
		// To compare with Unity transform and itself
		public override bool Equals(System.Object obj)
    	{
        	if (obj == null)
        	{
           	 	return false;
        	}
        	
	        Transform t = obj as Transform;
	        NetworkTransform n = obj as NetworkTransform;
	        
	        if (t!=null) {
	        	return (t.position == this.position && t.rotation==this.rotation);
	        }
	        else if (n!=null) {
	        	return (n.position == this.position && n.rotation==this.rotation);
	        }
	        else {
	        	return false;
	        }	        	
	   	}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		private void InitFromCurrent() {
			this.position = obj.transform.position;
			this.rotation = obj.transform.rotation;	
		}
		
		private void InitFromGiven(Transform trans) {
			this.position = trans.position;
			this.rotation = trans.rotation;	
		}
		
	}

