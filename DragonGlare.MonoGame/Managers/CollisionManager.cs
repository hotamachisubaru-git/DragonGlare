using System.Collections.Generic;
using DragonGlare.Entities;

namespace DragonGlare.Managers
{
    public static class CollisionManager
    {
        public static void CheckCollisions(Player player, List<Entity> enemies, List<Entity> bullets)
        {
            foreach (var enemy in enemies)
            {
                // プレイヤーと敵の衝突
                if (player.Bounds.Intersects(enemy.Bounds))
                {
                    // ダメージ処理など
                }

                // 弾と敵の衝突
                foreach (var bullet in bullets)
                {
                    if (bullet.Bounds.Intersects(enemy.Bounds))
                    {
                        bullet.IsActive = false;
                        enemy.IsActive = false;
                        // スコア加算など
                    }
                }
            }
        }
    }
}
