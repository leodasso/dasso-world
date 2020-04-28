using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;

public class WorldMapGuy : MonoBehaviour
{
    [ShowInInspector, ReadOnly]
    StagePoint _currentStagePoint;
    Player _player;
    Vector2 _playerInput;

    public void SetStagePoint(StagePoint newStagePoint)
    {
        _currentStagePoint = newStagePoint;
        transform.position = newStagePoint.transform.position;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _player = GameMaster.Player();
    }

    // Update is called once per frame
    void Update()
    {
        _playerInput = new Vector2(_player.GetAxis("moveH"), _player.GetAxis("moveV"));
        
        if (!_currentStagePoint) return;
    }
}