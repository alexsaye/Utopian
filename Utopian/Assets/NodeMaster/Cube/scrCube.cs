﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrCube : MonoBehaviour
{
	public LinkedListNode<GameObject> Cube { get; private set; }
	public bool Infected { get; private set; }
	public bool Uploading { get; private set; }
	public GameObject ExplosionPrefab;
	public scrNode Parent;

	float infectionTransitionDuration = 2.0f;
	float infectionTransitionTimer = 0.0f;
	bool infectionTransitionCompleted = false;

	float damageTimer = 0;
	float damageToDestroy = 2;

	public GameObject ChildSpark { get; private set; }

	scrPathfinder pathfinder;

	// Set infect over time flag.
	public void Infect()
	{
		Infected = true;
	}
	
	// Immediately infect.
	public void InfectImmediate()
	{
		Infected = true;
		infectionTransitionCompleted = true;
		renderer.material = scrNodeMaster.Instance.MatCubeInfected;
		ChildSpark.particleSystem.startColor = scrNodeMaster.ColCubeInfected;
	}

	public void Init(LinkedListNode<GameObject> cube, scrNode parent, Vector3 position, bool infected)
	{
		transform.position = position;
		transform.rotation = Quaternion.identity;
		Parent = parent;

		if (infected)
		{
			Infected = true;
			infectionTransitionCompleted = true;
			renderer.material = scrNodeMaster.Instance.MatCubeInfected;
			ChildSpark.particleSystem.startColor = scrNodeMaster.ColCubeInfected;
		}
		else
		{
			Infected = false;
			infectionTransitionCompleted = false;
			infectionTransitionTimer = 0.0f;
			renderer.material = scrNodeMaster.Instance.MatCubeUninfected;
			ChildSpark.particleSystem.startColor = scrNodeMaster.ColCubeUninfected;
		}

		damageTimer = 0;
		Uploading = false;
		pathfinder.Pause ();
	}

	public void Upload()
	{
		Uploading = true;
		pathfinder.Resume();
	}

	// Use this for initialization
	void Start ()
	{
		ChildSpark = transform.Find ("Spark").gameObject;
		pathfinder = GetComponent<scrPathfinder>();
		pathfinder.Target = GameObject.Find ("AICore");
		Init (null, null, Vector3.zero, false);
	}

	// Update is called once per frame
	void Update ()
	{
		if (Uploading)
		{
			// While uploading (pathing towards the core) check if within the core.
			if (false)//within the core)
			{
				scrNodeMaster.Instance.DeactivateCube(Cube);
			}

		}

		if (Infected)
		{
			if (!infectionTransitionCompleted)
			{
				infectionTransitionTimer += Time.deltaTime;
				if (infectionTransitionTimer >= infectionTransitionDuration)
				{
					infectionTransitionCompleted = true;
					renderer.material = scrNodeMaster.Instance.MatCubeInfected;
					ChildSpark.particleSystem.startColor = scrNodeMaster.ColCubeInfected;
				}
				else
				{
					// Interpolate between the colours of the materials with a unique material.
					float transition = infectionTransitionTimer / infectionTransitionDuration;
					renderer.material.SetColor("_GlowColor", Color.Lerp(scrNodeMaster.ColCubeUninfected, scrNodeMaster.ColCubeInfected, transition));
				}
			}
		}

		if (damageTimer > damageToDestroy)
		{
			GameObject explosion = (GameObject)Instantiate (ExplosionPrefab, transform.position, Quaternion.identity);
			explosion.particleSystem.startColor = Color.Lerp (renderer.material.GetColor("_GlowColor"), Color.white, 0.5f);
			Parent.RemoveCube(gameObject);
		}
		else
		{
			if (damageTimer < 0)
			{
				damageTimer = 0;

				if (Infected)
					renderer.material = scrNodeMaster.Instance.MatCubeInfected;
				else
					renderer.material = scrNodeMaster.Instance.MatCubeUninfected;
			}
			else if (damageTimer > 0)
			{
				damageTimer -= 2 * Time.deltaTime;
				renderer.material.color = Color.Lerp(Color.black, Color.white, damageTimer / damageToDestroy);
			}
		}
	}

	void OnCollisionEnter(Collision c)
	{
		if (c.gameObject.layer == LayerMask.NameToLayer("PBullet"))
		{
			scrBullet bullet = c.gameObject.GetComponent<scrBullet>();
			damageTimer += bullet.Damage;
			ChildSpark.transform.forward = -bullet.Direction;
			ChildSpark.particleSystem.Emit (10);
		}
	}
}
