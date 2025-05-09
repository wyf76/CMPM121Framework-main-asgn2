using UnityEngine;
using System;

public class BaseSpell : Spell
{
    private int damage, secondaryDamage, manaCost;
    private float cooldown;
    private Damage.Type damageType;
    private string trajectory;
    private float speed, lifetime;
    private int sprite;
    private int projectileCount, secondaryCount;

    public BaseSpell(string name, string desc, int icon,
                     int dmg, int secDmg, int cost, float cd, Damage.Type dmgType,
                     string traj, float spd, float life, int spr,
                     int projCount = 1, int secCount = 0)
    {
        Name = name; Description = desc; IconIndex = icon;
        damage = dmg; secondaryDamage = secDmg; manaCost = cost; cooldown = cd;
        damageType = dmgType;
        trajectory = traj; speed = spd; lifetime = life; sprite = spr;
        projectileCount = projCount; secondaryCount = secCount;
    }

    internal override void CastInternal(Vector3 pos, Vector3 dir, SpellModifierContext ctx)
    {
        float finalDmg = ctx.ApplyDamage(damage);
        float finalSpeed = ctx.ApplySpeed(speed);
        float finalLife = ctx.ApplyLifetime(lifetime);
        string finalTraj = ctx.overrideTrajectory ?? trajectory;
        int finalSpr = ctx.overrideSprite != 0 ? ctx.overrideSprite : sprite;

        // spawn logic (single or multiple)
        int count = projectileCount;
        float angleSpread = 60f;
        for (int i = 0; i < count; i++)
        {
            Vector3 dir2 = dir;
            if (count > 1)
            {
                float angle = -angleSpread/2 + angleSpread*(i/(float)(count-1));
                dir2 = Quaternion.Euler(0,0,angle)*dir;
            }
            Spawn(pos, dir2.normalized, finalDmg, finalSpeed, finalLife, finalTraj, finalSpr, ctx);
        }
    }

    void Spawn(Vector3 pos, Vector3 dir, float dmg, float spd, float life, string traj, int spr, SpellModifierContext ctx)
    {
        Action<Hittable,Vector3> onHit = (hit, point) => {
            if (hit != null)
                hit.Damage(new Damage(Mathf.RoundToInt(dmg), damageType));

            if (secondaryCount>0)
            {
                float secD = secondaryDamage>0? ctx.ApplyDamage(secondaryDamage):dmg;
                for(int j=0;j<secondaryCount;j++)
                {
                    float ang=360f*j/secondaryCount;
                    Vector3 d2=Quaternion.Euler(0,0,ang)*Vector3.right;
                     Action<Hittable,Vector3> secHit = (h, p) =>{
                        if (h != null)
                            h.Damage(new Damage(Mathf.RoundToInt(secD),damageType));};
                    ProjectileManager.Instance.CreateProjectile(spr, traj, point, d2, ctx.ApplySpeed(speed), life, secHit);
                }
            }
        };
        ProjectileManager.Instance.CreateProjectile(sprite, trajectory, pos, dir, ctx.ApplySpeed(speed), lifetime, onHit);
    }

    public override int GetManaCost()    => manaCost;
    public override float GetDamage()    => damage;
    public override float GetCooldown()  => cooldown;
    public override string GetProjectileType()   => trajectory;
    public override int GetProjectileSprite()    => sprite;
    public override float GetProjectileSpeed()   => speed;
    public override float GetProjectileLifetime() => lifetime;
}
