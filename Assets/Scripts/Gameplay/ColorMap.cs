using UnityEngine;
using System.Collections.Generic;

public class ColorMap : MonoBehaviour
{
    [SerializeField] private Color None;
    [SerializeField] private Color Normal;
    [SerializeField] private Color Fire;
    [SerializeField] private Color Water;
    [SerializeField] private Color Grass;
    [SerializeField] private Color Wind;
    [SerializeField] private Color Electric;
    [SerializeField] private Color Ice;
    [SerializeField] private Color Metal;
    [SerializeField] private Color Light;
    [SerializeField] private Color Dark;
    [SerializeField] private Color Evil;
    [SerializeField] private Color Dragon;
    [SerializeField] private Color Chaos;
    [SerializeField] private Color Holy;

    private Dictionary<DeityMonsType, Color> colorMap;

    void Start()
    {
        colorMap = new Dictionary<DeityMonsType, Color>
        {
            {DeityMonsType.None, None},
            {DeityMonsType.Normal, Normal},
            {DeityMonsType.Fire, Fire},
            {DeityMonsType.Water, Water},
            {DeityMonsType.Grass, Grass},
            {DeityMonsType.Wind, Wind},
            {DeityMonsType.Electric, Electric},
            {DeityMonsType.Ice, Ice},
            {DeityMonsType.Metal, Metal},
            {DeityMonsType.Light, Light},
            {DeityMonsType.Dark, Dark},
            {DeityMonsType.Evil, Evil},
            {DeityMonsType.Dragon, Dragon},
            {DeityMonsType.Chaos, Chaos},
            {DeityMonsType.Holy, Holy}
        };
    }

    public Color GetColorByType(DeityMonsType type)
    {
        if (colorMap.TryGetValue(type, out Color color))
        {
            return color;
        }
        else
        {
            return Color.white;
        }
    }
}