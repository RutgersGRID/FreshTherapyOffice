using UnityEngine;
using System.Collections;

//ZPWH
public class CameraController : MonoBehaviour 
{
	public Transform[] movePath;
	public Transform[] lookPath;
	public Transform lookTarget;
	public float percentage;
	public float time=0;
	//ZPWH: if it becomes necessary to add ping pong tours in uncomment
	//public string tourType = null;
	private float redPosition = .16f;
	private float bluePosition = .53f;
	private float greenPosition = 1;
	
	static float goToPos = 0f;
	static int wait = 0;
	
	public Font font;
	private GUIStyle style = new GUIStyle();
	
	void Start()
	{
		if(time==0)
		{
			time=35;
		}
		style.font=font;
		SlideTo(1f);
		
		//ZPWH: if it becomes necessary to add ping pong tours in uncomment
		/*if (tourType.Equals(null))
		{
			tourType = "loop";
		}*/
	}
	
	void Update()
	{
		//ZPWH: if it becomes necessary to add ping pong tours in uncomment
		//if(tourType.Equals("loop"))
		//{
		
		
			//This sets up tour in loop
			if(wait>100)
			{
				if(percentage==1f)
				{
					percentage=0f;
				}
				else if(percentage==0f)
				{
					SlideTo(1f);
					wait = 0;
				}	
			}
			wait++;
		//}
		
		//ZPWH: if it becomes necessary to add ping pong tours in uncomment
		/*else if(tourType.Equals("pong"))
		{
			//This sets up tour as ping pong rather than current looping
			if(percentage==1f)
			{
				percentage = .999999f;
				SlideTo(0f);
			}
			if(percentage==0f)
			{
				percentage = .000001f;
				SlideTo(1f);
			}
		}*/
	}
		
	void OnGUI()
	{
		//moved original sliders off of the screen
		
		percentage=GUI.VerticalSlider(new Rect(Screen.width-20,200000,15,Screen.height-40),percentage,1,0);
		iTween.PutOnPath(gameObject,movePath,percentage);
		iTween.PutOnPath(lookTarget,lookPath,percentage);
		transform.LookAt(iTween.PointOnPath(lookPath,percentage));
		
		if(GUI.Button(new Rect(50000,Screen.height-25,50,20),"Red"))
		{
			SlideTo(redPosition);
		}
		if(GUI.Button(new Rect(60000,Screen.height-25,50,20),"Blue"))
		{
			SlideTo(bluePosition);
		}
		if(GUI.Button(new Rect(11500,Screen.height-25,50,20),"Green"))
		{
			SlideTo(greenPosition);
		}
	}
	
	void OnDrawGizmos(){
		iTween.DrawPath(movePath,Color.magenta);
		iTween.DrawPath(lookPath,Color.cyan);
		Gizmos.color=Color.black;
		Gizmos.DrawLine(transform.position,lookTarget.position);
	}
	
	void SlideTo(float position)
	{
		//iTween.Stop(gameObject);
		iTween.ValueTo(gameObject,iTween.Hash("from",percentage,"to",position,"time",time,"easetype",iTween.EaseType.easeInOutQuad,"onupdate","SlidePercentage"));
	}
	
	void SlidePercentage(float p)
	{
		percentage=p;
	}
}
