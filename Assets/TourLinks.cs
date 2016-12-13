using UnityEngine;
using System.Collections;

//ZPWH this script is so when user wants to go on tours the camera changes
//tours do not start until the user has selected them
//when user changes tour or exits tour the current tour will pause in place until returned to
public class TourLinks : MonoBehaviour 
{
	public GameObject outsideTour;  
	public GameObject insideTour;  
	public GameObject fromBeckTour;
	public GameObject circleTour; 
	
	public GameObject tour1Camera;
	public GameObject tour2Camera;
	public GameObject tour3Camera;
	public GameObject tour4Camera;
	GameObject mainCam;
	
	public bool inTour = false;
	
	public void Start()
	{
		mainCam = GameObject.FindGameObjectWithTag("MainCamera");
		
		outsideTour.SetActiveRecursively(false);
		circleTour.SetActiveRecursively(false);
		fromBeckTour.SetActiveRecursively(false);
		insideTour.SetActiveRecursively(false);
	}
	public void Update()
	{
		if(mainCam==null)
		{
			mainCam = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}
	public void SwitchToTour1Camera()
	{
		inTour = true;
		outsideTour.SetActiveRecursively(true);
		
		mainCam.GetComponent<Camera>().enabled = false;
		
		circleTour.SetActiveRecursively(false);
		fromBeckTour.SetActiveRecursively(false);
		insideTour.SetActiveRecursively(false);
	}
	
	public void SwitchToTour2Camera()
	{
		inTour = true;
		insideTour.SetActiveRecursively(true);
		
		mainCam.GetComponent<Camera>().enabled = false;
		
		outsideTour.SetActiveRecursively(false);
		circleTour.SetActiveRecursively(false);
		fromBeckTour.SetActiveRecursively(false);
	}
	
	public void SwitchToTour3Camera()
	{
		inTour = true;
		fromBeckTour.SetActiveRecursively(true);
		
		mainCam.GetComponent<Camera>().enabled = false;
		
		outsideTour.SetActiveRecursively(false);
		circleTour.SetActiveRecursively(false);
		insideTour.SetActiveRecursively(false);
	}
	
	public void SwitchToTour4Camera()
	{
		inTour = true;
		circleTour.SetActiveRecursively(true);
		
		mainCam.GetComponent<Camera>().enabled = false;
		
		outsideTour.SetActiveRecursively(false);
		fromBeckTour.SetActiveRecursively(false);
		insideTour.SetActiveRecursively(false);
	}
	
	public void PlayerCamera()
	{
		inTour = false;
		mainCam.active = true;
		mainCam.GetComponent<Camera>().enabled = true;
		
		outsideTour.SetActiveRecursively(false);
		circleTour.SetActiveRecursively(false);
		fromBeckTour.SetActiveRecursively(false);
		insideTour.SetActiveRecursively(false);
	}
	
	public bool InTour()
	{
		return inTour;
	}
}
