using UnityEngine;

// LA INTERFAZ (El contrato de la Estrategia)
public interface IEnemyStrategy
{
    void Execute(EnemyAI context);
}
