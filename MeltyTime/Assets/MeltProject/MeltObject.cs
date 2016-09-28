using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MeltObject : MonoBehaviour {

	Vector3 downVec = new Vector3 (0, -1, 0);

	public float max;


	//The mesh
	Mesh mesh;

	Mesh collisionMesh;
	int collision;
//	float collisionMaxX;
//	float collisionMinX;
//	float collisionMaxZ;
//	float collisionMinZ;

	float bottom;

	GameObject[] melters;

	float maxDist;
	float minDist;


	float proximityModifier;

	//All the vertices in the mesh
	Vector3[] vertices;

	//List of vertex connectons
	List<int>[] connections;
	float[,] distances;

	// Use this for initialization
	void Start () {

		melters = GameObject.FindGameObjectsWithTag ("melter");

		proximityModifier = 1f;

		for (int p = 0; p < melters.Length; p++) {
			proximityModifier -= Vector3.Distance (melters [p].GetComponent<Renderer> ().bounds.center,
				GetComponent<Renderer> ().bounds.center) / 100;
//			Debug.Log(Vector3.Distance(melters[p].GetComponent<Renderer>().bounds.center,
//				GetComponent<Renderer>().bounds.center));
			
		}


		bottom = 100000f;



		mesh = GetComponent<MeshFilter>().mesh;

		vertices = mesh.vertices;
		connections = new List<int>[vertices.Length];
		distances = new float[vertices.Length, vertices.Length];

		//initialize connection lists
		for (int x = 0; x < connections.Length; x++) {
			connections [x] = new List<int> ();
		}

		minDist = 20000f;
		maxDist = -1000f;
		for (int p = 0; p < vertices.Length; p++) {
			for (int q = 0; q < vertices.Length; q++) {
				distances [p, q] = Vector3.Distance (vertices [p], vertices [q]);
				if (distances [p, q] < minDist && distances[p, q] != 0)
					minDist = distances [p, q];
			}
		}

			
	}
	
	// Update is called once per frame
	void Update() {

		if (collision <= 0)
			return;

		if (GetComponent<Renderer> ().bounds.size.x > max)
			return;
//		if (GetComponent<Renderer> ().bounds.size.y > max/2)
//			return;
		if (GetComponent<Renderer> ().bounds.size.z > max)
			return;
		
		
		int i = 0;

		for (int k = 0; k < vertices.Length; k++) {
			if (distances [i, k] < minDist && distances [i, k] != 0) {
				return;
			}
		}
		while (i < vertices.Length) {

			float x = transform.TransformPoint(vertices[i]).x;
			float y = transform.TransformPoint(vertices[i]).y;
			float z = transform.TransformPoint(vertices[i]).z;

			if (y > bottom) {
				
				var sum = -Vector3.up * Time.deltaTime * proximityModifier * (max - GetComponent<Renderer>().bounds.size.x)/max;

				vertices [i] += sum;

			} else {

				Vector3 norm = vertices [i].normalized;

				Vector3 n = new Vector3 (0, 0, 0);
				n.x = norm.x;
				n.y = 0;
				n.z = norm.z;

				RaycastHit hit;
//				Debug.DrawRay (transform.TransformPoint(vertices [i]), vertices[i].normalized, Color.red);
				if (!Physics.Raycast (transform.TransformPoint(vertices [i]), -Vector3.up, out hit, 0.1f)) {

					n = -Vector3.up;

					RaycastHit secondHit;
					if (Physics.Raycast (transform.TransformPoint(vertices [i]), 
						new Vector3(-vertices[i].normalized.x, 0, -vertices[i].normalized.z), 
						out secondHit, 0.2f)) {
						vertices[i] += new Vector3(vertices[i].normalized.x + 0.5f, 0, vertices[i].normalized.z + 0.5f) * 0.3f * Time.deltaTime;
					}

						
				}

//				RaycastHit thirdHit;
//				if (Physics.Raycast (transform.TransformPoint (vertices [i]) + new Vector3(0, 0.1f, 0), 
//					new Vector3 (vertices[i].normalized.x, 0, vertices[i].normalized.z), 
//					out thirdHit, 0.1f)) {
//
//
//					n = -n;
//				}

				vertices [i] += n * Time.deltaTime * proximityModifier * (max - GetComponent<Renderer>().bounds.size.x)/max;

			}

			i++;
		}

		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

	void OnCollisionEnter(Collision other) {

		this.GetComponent<Rigidbody>().isKinematic = true;
		collision++;
		collisionMesh = other.collider.gameObject.GetComponent<MeshFilter> ().mesh;
		Vector3[] collVertices = collisionMesh.vertices;

		for (int i = 0; i < vertices.Length; i++) {
			if (transform.TransformPoint(vertices [i]).y < bottom)
				bottom = transform.TransformPoint(vertices [i]).y;
		}
		bottom += 0.2f;

		for (int k = 0; k < vertices.Length; k++) {
			vertices [k].y += 0.2f;
		}


		foreach(Collider c in GetComponents<Collider> ()) {
			c.enabled = false;
		}

	}
						
	void OnCollisionExit(Collision collisionInfo) {
		collision--;
	}



}
