using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class BehaviourPlayer : MonoBehaviour
{
    NavMeshAgent agent;
    Player player;
    UI ui;

    Vector2 move;

    bool isFire;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GetComponent<Player>();
        player.onPerkAvailable += Perk_Availabled;
    }

    private void Perk_Availabled(List<PerkID> usedPerks)
    {
        ui.ShowPerkPanel(usedPerks);
    }

    public void SetUI(UI ui)
    {
        this.ui = ui;

        this.ui.onJoystick += Joystick_Raised;
        this.ui.onPerkClick += Perk_Clicked;
    }

    private void Perk_Clicked(PerkID perkID)
    {
        player.PickPerk(perkID);
    }

    private void Joystick_Raised(Vector2 value)
    {
        move = value;
        //move.Normalize();
       
        //agent.SetDestination(transform.position + (new Vector3(move.x, 0, move.y) * 1));
        //agent.Move(new Vector3(move.x, 0, move.y) * Time.deltaTime * agent.speed);

        isFire = value == Vector2.zero;
    }

    private void Update()
    {
        if (player.IsMine())
        {
            if (Input.GetKey(KeyCode.W))
            {
                move.y = 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                move.x = -1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                move.y = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                move.x = 1;
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                move.y = 0;
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                move.x = 0;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                move.y = 0;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                move.x = 0;
            }

            move.Normalize();
            
            //agent.SetDestination(transform.position + (new Vector3(move.x, 0, move.y) * 1));
            agent.Move(agent.speed * Time.deltaTime * new Vector3(move.x, 0, move.y));

            isFire = move == Vector2.zero;

            if (isFire)
            {
                player.Fire();
            }
        }
    }

    private void OnDestroy()
    {
        if (ui)
        {
            ui.onJoystick  -= Joystick_Raised;
            ui.onPerkClick -= Perk_Clicked;
        }
    }
}
