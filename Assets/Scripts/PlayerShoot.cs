﻿using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;
    [SerializeField]
    private GameObject[] prefabs;

    private WeaponManager weaponManager;
    private PlayerWeapon curWeapon;

    private void Start()
    {
        if (!cam)
        {
            Debug.LogError("PlayerShoot: No camera referenced");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();

        //-----------------------------------------------------------
        //fixing graphics if there is an object with clildren inside
        //weaponGFX.layer = LayerMask.NameToLayer(weaponLayerName);
        //Transform parent = weaponGFX.transform;
        //foreach (Transform child in parent)
        //    { child.gameObject.layer = LayerMask.NameToLayer(weaponLayerName); }
        //-------------------------------------------------------------



        //weaponGFX.layer = LayerMask.NameToLayer(weaponLayerName);
    }

    private void Update()
    {
        curWeapon = weaponManager.GetCurrentWeapon();

        if(curWeapon.fireRate <= 0f)
        {
            if (Input.GetButtonDown("Fire1"))    //make shoot via left click of the mouse
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f/curWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayerRayCast();
        }
    }

    //Is called on the server when a player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    //is called on all clients when we need to do a shoot effect
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    [Client]
    void Shoot()
    {
        if (!isLocalPlayer)
            return;

        Debug.Log("Shoot");

        //We are shooting, call the OnShoot method on server
        CmdOnShoot();

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, curWeapon.range, mask))
        {
            //our bullet hits object in range of weapon if it has correct property
            if(hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(hit.collider.name, curWeapon.damage);
            }
            //Debug.Log("We hit" + hit.collider.name);
        }
    }

    [Command]
    void CmdPlayerShot(string playerID, int damage)
    {
        Debug.Log(playerID + " has been shot");
        //Destroy(GameObject.Find(ID));
        Player player = GameManager.GetPlayer(playerID);    //find player that gets damage
        player.RpcTakeDamage(damage);    //taking damage from shooting
    }

    #region createObject

    [System.Obsolete]
    public void PlayerRayCast()
    {

        RaycastHit hit;

        //Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 3.0f, mask))
        {
            Transform objectHit = hit.transform;
            CmdCreateObject(prefabs[0], hit.point, hit.transform.position, hit.transform.localScale);


            //created.transform.position = detectSpawnPosition(hitPosition, objPosition, hit.transform.localScale, created.transform.localScale);


            // Do something with the object that was hit by the raycast.
        }

    }

    Vector3 detectSpawnPosition(Vector3 hitPosition, Vector3 objPosition, Vector3 hittedObjScale, Vector3 creatingObjScale)
    {

        double[] diferrences = new double[3];

        diferrences[0] = Mathf.Abs(hitPosition.x - objPosition.x);
        diferrences[1] = Mathf.Abs(hitPosition.y - objPosition.y);
        diferrences[2] = Mathf.Abs(hitPosition.z - objPosition.z);

        int i = diferrences.ToList().IndexOf(diferrences.Max());

        switch (i)
        {
            case 0:
                return new Vector3(
                    objPosition.x + (hittedObjScale.x * 0.5f * (hitPosition.x - objPosition.x) > 0 ? 1 : -1),
                    objPosition.y,
                    objPosition.z);
            case 1:
                return new Vector3(
                    objPosition.x,
                    objPosition.y + (hittedObjScale.y * 0.5f * (hitPosition.y - objPosition.y) > 0 ? 1 : -1),
                    objPosition.z);
            case 2:
                return new Vector3(
                    objPosition.x,
                    objPosition.y,
                    objPosition.z + (hittedObjScale.z * 0.5f * (hitPosition.z - objPosition.z) > 0 ? 1 : -1));

        }
        return Vector3.zero;
    }

    [Command]
    void CmdCreateObject(GameObject prefab, Vector3 hitPos, Vector3 objPos, Vector3 hitScale)
    {
        GameObject obj = (GameObject)Instantiate(prefab);

        //Spawn the bullet on the clients
        NetworkServer.Spawn(obj);

        obj.transform.position = detectSpawnPosition(hitPos, objPos, hitScale, obj.transform.localScale);
    }

    #endregion
}
