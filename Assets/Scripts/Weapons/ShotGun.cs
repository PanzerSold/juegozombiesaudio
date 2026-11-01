using System.Collections.Generic;
using UnityEngine;

public class ShotGun : GunBehaviour
{
    [Header("Shotgun Settings")]
    [SerializeField] private int raycastCount = 8;
    [SerializeField] private float spreadAngle = 6f;
    [SerializeField] private float rotationPerShot = 15f;

    [Header("Impact Particles")]
    [SerializeField] private ParticleSystem impactPrefab;

    private float currentRotation = 0f;

    public override void Shoot()
    {
        AudioClip clipToPlay = player.activePerks[7] ? shooteExplosiveClip : shootClip;
        shootSfx.pitch = Random.Range(minPitchShift, maxPitchShift);
        
        FireAmmo();

        for (int i = 0; i < raycastCount; i++)
        {

            float angle = currentRotation + i * (360 / raycastCount);

            float radians = angle * Mathf.Deg2Rad;

            float radius = Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

            Vector3 offset = cameraPos.transform.right * Mathf.Cos(radians) + cameraPos.transform.up * Mathf.Sin(radians);

            Vector3 direction = (cameraPos.transform.forward + offset * radius).normalized;

            ShootRaycast(direction);

        }

        currentRotation = Mathf.Repeat(currentRotation + rotationPerShot, 360f);
        shootSfx.PlayOneShot(clipToPlay);
    }



    private void ShootRaycast(Vector3 direction)
    {
        Vector3 origin = cameraPos.transform.position;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastDistance, coll))
        {
            ParticleSystem impact = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));

            ParticleSystemRenderer renderer = impact.GetComponent<ParticleSystemRenderer>();

            renderer.material = materialGenerico;

            if (player.activePerks[7])
            {
                float explosiveDamage = gunDamage * 2;
                Collider[] colliders = Physics.OverlapSphere(hit.point, boomRadius);
                List<ZombieBase> currentZombie = new List<ZombieBase>();
                foreach (ParticleSystem pse in psExplosiveAmmo)
                {
                    pse.transform.position = hit.point;
                    pse.Play();
                }

                foreach (Collider hitCollider in colliders)
                {
                    ZombieBase zombie = hitCollider.gameObject.GetComponentInParent<ZombieBase>();
                    if (zombie != null)
                    {
                        if (currentZombie.Contains(zombie)) continue;
                        currentZombie.Add(zombie);
                        zombie.ReceiveDamage(false, explosiveDamage);
                    }
                }
            }

            if (hit.collider.CompareTag("ZombieBody"))
            {
                ZombieBase zombie = hit.collider.GetComponentInParent<ZombieBase>();
                zombie.ReceiveDamage(false, gunDamage);

                renderer.material = materialZombieHit;

            }

            if (hit.collider.CompareTag("ZombieHead"))
            {
                ZombieBase zombie = hit.collider.GetComponentInParent<ZombieBase>();
                zombie.ReceiveDamage(true, gunDamage);

                renderer.material = materialZombieHit;
            }

            impact.Play();

            ParticleSystem.MainModule main = impact.main;

            float lifetime = main.duration +
                             main.startLifetime.constantMax;

            Destroy(impact.gameObject, lifetime);
        }
    }
}