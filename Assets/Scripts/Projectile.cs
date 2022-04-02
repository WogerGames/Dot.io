using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Projectile : MonoBehaviourPun
{
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float speed = 15f;

    public int OwnerId { get; set; }

    [field: SerializeField]
    public Team Team { get; set; }

    Vector3 prevPoint, curPoint;
    Vector3 moveDir;
    float lofetime;
    int dmg;
    ProjectileData projectileData;

    public void Init(ProjectileData data, bool yFix = true)
    {
        if (yFix)
            data.direction.y = 0;

        prevPoint = transform.position;
        curPoint = transform.position;

        projectileData = data;

        dmg = data.baseDamege;
        moveDir = data.direction;
        OwnerId = data.ownerID;
        Team = data.team;
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * moveDir;

        lofetime += Time.deltaTime;

        if (photonView.IsMine)
        {
            if (lofetime > 1.5f)
            {
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }

            CheckCollision();
        }

    }

    void CheckCollision()
    {
        Vector3 dir = curPoint - prevPoint;

        var hits = Physics.RaycastAll(prevPoint, dir, dir.magnitude * 1.5f, collisionMask);

        hits = hits.ToList().Where(h =>
        {
            var health = h.collider.transform.root.GetComponent<HealthComponent>();

            if (health && health.OwnerID != OwnerId && health.Team != Team)
                return true;

            return false;

        }).ToArray();

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                CalculateDamage();

                var h = hit.collider.transform.root.GetComponent<HealthComponent>();
                h.Damage(dmg, OwnerId);
                break;
            }

            PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }

        prevPoint = curPoint;
        curPoint = transform.position;
    }

    void CalculateDamage()
    {
        int critChance = projectileData.critChance;
        if (Random.Range(0, 100) < critChance)
        {
            dmg *= 2;
        }
    }
}

public class ProjectileData
{
    public Vector3 direction;
    public int baseDamege = 1;
    public int critChance = 5;
    public int ownerID;
    public Team team;
}
