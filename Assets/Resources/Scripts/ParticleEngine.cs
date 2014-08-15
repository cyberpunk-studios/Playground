﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class ParticleEngine : MonoBehaviour {

  public int numberOfParticles = 1000;
  public float minSpeed = 1.0f;
  public float dampen = 0.5f;
  public float attraction = 0.1f;
  public float size = 0.5f;
  public Color slowColor = Color.white;
  public Color fastColor = Color.black;
  
  private Controller leap_controller_;
  private ParticleSystem particle_system_;
  
  private const float height_ = 20.0f;

	// Use this for initialization
	void Start () {
    leap_controller_ = new Controller();
    particle_system_ = (ParticleSystem)(GameObject.Find("Particles").GetComponent(typeof(ParticleSystem)));
	}
	
	// Update is called once per frame
	void Update () {
    // Set particle system variables
    float lifetime = height_ / minSpeed;
    particle_system_.maxParticles = numberOfParticles;
    particle_system_.emissionRate = numberOfParticles / lifetime;
    particle_system_.startSpeed = minSpeed;
    particle_system_.startLifetime = lifetime;
    particle_system_.startSize = size;

    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particle_system_.particleCount];
    particle_system_.GetParticles(particles);
    Vector3 starting_pos = particle_system_.transform.position - new Vector3(0.0f, 0.0f, 0.5f);

    List<Vector3> list_palm_vel = new List<Vector3>();
    List<Vector3> list_palm_pos = new List<Vector3>();
    Frame frame = leap_controller_.Frame();
    for (int i = 0; i < frame.Hands.Count; ++i)
    {
      Vector3 temp_pos = frame.Hands[i].PalmPosition.ToUnityScaled() * 10;
      Vector3 palm_pos = new Vector3(temp_pos.x, -temp_pos.z, temp_pos.y);
      list_palm_pos.Add(palm_pos);

      Vector3 temp_vel = frame.Hands[i].PalmVelocity.ToUnityScaled();
      Vector3 palm_vel = new Vector3(temp_vel.x, -temp_vel.z, temp_vel.y);
      list_palm_vel.Add(palm_vel);
    }

    for (int i = 0; i < particles.Length; ++i)
    {
      particles[i].startLifetime = particle_system_.startLifetime;
      particles[i].size = particle_system_.startSize;
      Vector3 starting_vel = (particles[i].position - starting_pos).normalized * minSpeed;
      Vector3 palm_effect = Vector3.zero;
      float total_ratio = 0;
      for (int j = 0; j < list_palm_pos.Count; ++j)
      {
        Vector3 palm_vel = list_palm_vel[j];
        Vector3 palm_pos = list_palm_pos[j];
        float distance = Vector3.Distance(particles[i].position, palm_pos);
        float magnitude = palm_vel.magnitude;
        float ratio = Mathf.Min(0.5f, magnitude / Mathf.Pow(distance, 6));
        palm_effect += ratio * palm_vel * 4;
        total_ratio += ratio;
      }
      particles[i].velocity = palm_effect + (1 - total_ratio) * particles[i].velocity;
      particles[i].velocity = 0.8f * particles[i].velocity + 0.2f * starting_vel;
    }

    particle_system_.SetParticles(particles, particles.Length);
	}
}
