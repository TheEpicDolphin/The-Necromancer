using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firearm : MonoBehaviour
{
    float coolDown = 0.5f;
    float nextAttackTime;
    GameObject projectilePath;
    Transform projectileOrigin;
    // Start is called before the first frame update
    void Start()
    {
        projectileOrigin = transform.GetChild(0);

        projectilePath = new GameObject();
        LineRenderer lineRend = projectilePath.AddComponent<LineRenderer>();
        projectilePath.transform.parent = projectileOrigin;
        projectilePath.transform.localPosition = Vector3.zero;
        projectilePath.transform.localRotation = Quaternion.identity;
        
        lineRend.material = new Material(Shader.Find("Sprites/Default"));
        lineRend.material.color = Color.red;
        lineRend.widthMultiplier = 0.1f;
        lineRend.positionCount = 2;
        lineRend.useWorldSpace = false;
        lineRend.SetPosition(0, Vector3.zero);
        lineRend.SetPosition(1, 1000.0f * Vector3.forward);
    }

    private void OnDestroy()
    {
        Destroy(projectilePath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot(Animator animator, ref Inventory inventory)
    {

        if (inventory.handgunAmmo > 0)
        {
            if (Time.time > nextAttackTime)
            {
                nextAttackTime = Time.time + coolDown;

                //Raycast to shooting location
                Ray ray = new Ray(projectileOrigin.position, transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    //Hitable hitableObject = hit.collider.gameObject.GetComponent<Hitable>();
                    //hitableObject.ReceiveShot(dir, 200.0f, hit.point);
                    Debug.Log("HIT " + hit.collider.name);


                }
                Debug.DrawRay(projectileOrigin.position, 10.0f * transform.forward, Color.cyan, 0.5f);
                //Animate player
                //animator.SetTrigger(Animator.StringToHash("Fire"));
                inventory.handgunAmmo -= 1;

                //TODO: Animate gun
                StartCoroutine(ShootCoroutine());
            }
            
        }
        else
        {
            //No ammo or gun is currently firing
        }
    }

    IEnumerator ShootCoroutine()
    {
        //Wait for animation to finish

        yield return null;
        //Finished swinging. Disable hurtBox again
        //hurtBox.enabled = false;
    }
}
