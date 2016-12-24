using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZombieCounter : MonoBehaviour
{
    private Text m_Text;
    private int zombieCount;
    private int crawlerCount;
    private int tankCount;
    const string display = "{0} Zombies Alive\n{1} Crawlers Alive\n{2} Tanks Alive";

    private float debugLastTime = 0.0f;
    private float debugInterval = 10.0f;

    // Use this for initialization
    void Start ()
    {
        m_Text = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        zombieCount = 0;
        zombieCount = GameObject.FindGameObjectsWithTag("Zombie").GetLength(0);

        crawlerCount = 0;
        crawlerCount = GameObject.FindGameObjectsWithTag("Crawler").GetLength(0);

        tankCount = 0;
        tankCount = GameObject.FindGameObjectsWithTag("Tank").GetLength(0);

        m_Text.text= string.Format(display, zombieCount, crawlerCount, tankCount);

        // EDEBUG Update frame rate every 'debugInterval'
        if ((Time.time - debugLastTime) > debugInterval)
        {
            debugLastTime = Time.time;
            EmailDebugger.DebugLogAppend("     ZCT: (" + zombieCount + " , " + crawlerCount + " , " + tankCount + ")");
        }
    }
}
