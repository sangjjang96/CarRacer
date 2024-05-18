using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public enum DriveType
{
	RearWheelDrive,
	FrontWheelDrive,
	AllWheelDrive
}

public class WheelDrive : MonoBehaviour
{
	public AudioSource audioidle;
	public AudioSource audiolow;
	public AudioSource audiomid;
	public AudioSource audiohigh;
	public AudioClip idlesound;
	public AudioClip lowsound;
	public AudioClip midsound;
	public AudioClip highsound;
	public Transform Target;
	public Rigidbody rb;
	public GameObject enemy;
	public GameObject start;
	float currentTime;
	bool p = false;

    [Tooltip("Maximum steering angle of the wheels")]
	public float maxAngle = 30f;
	[Tooltip("Maximum torque applied to the driving wheels")]
	public float maxTorque = 300f;
	[Tooltip("Maximum brake torque applied to the driving wheels")]
	public float brakeTorque = 30000f;
	[Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
	public GameObject wheelShape;

	[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
	public float criticalSpeed = 5f;
	[Tooltip("Simulation sub-steps when the speed is above critical.")]
	public int stepsBelow = 5;
	[Tooltip("Simulation sub-steps when the speed is below critical.")]
	public int stepsAbove = 1;

	[Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
	public DriveType driveType;

    private WheelCollider[] m_Wheels;

    // Find all the WheelColliders down in the hierarchy.
	void Start()		// 시작시 wheel을 instantiate하여 위치에 가져다 놓음
	{
		rb = GetComponent<Rigidbody>();

		m_Wheels = GetComponentsInChildren<WheelCollider>();

		for (int i = 0; i < m_Wheels.Length; ++i) 
		{
			var wheel = m_Wheels [i];

			// Create wheel shapes only when needed.
			if (wheelShape != null)
			{
				var ws = Instantiate (wheelShape);
				ws.transform.parent = wheel.transform;
			}
		}
	}

	// This is a really simple approach to updating wheels.
	// We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
	// This helps us to figure our which wheels are front ones and which are rear.
	void Update()
	{
		GameObject fmObject = GameObject.Find("FinishManager");
		FinishManager fm = fmObject.GetComponent<FinishManager>();
		GameObject tmObject = GameObject.Find("TimeManager");
		TimeManager tm = tmObject.GetComponent<TimeManager>();
		GameObject stmObject = GameObject.Find("StartTimeManager");
		StartTimeManager stm = stmObject.GetComponent<StartTimeManager>();
		GameObject smObject = GameObject.Find("SpeedManager");
		SpeedManager sm = smObject.GetComponent<SpeedManager>();

		currentTime += Time.deltaTime;
		
		// 시작전 초 세기
		if(currentTime <= 1.0f)		
		{
			stm.StartUI.text = "3";
		}

		else if(currentTime <= 2.0f)
		{
			stm.StartUI.text = "2";
		}

		else if(currentTime <= 3.0f)
		{
			stm.StartUI.text = "1";
		}


		if(transform.position.z >= 370 && enemy.transform.position.z < 368)  // Player가 Finish line에 Enemy보다 먼저 도착하면 Win UI표시
		{
			fm.FinishUI.text = "Win";
			p = true;		// Player가 이기면 boolean변수 p를 true로 하여 player가 이겼음을 표시
		}

		else if(transform.position.z < 370 && enemy.transform.position.z >= 368)
		{
			fm.FinishUI.text = "Lose";
		}

		else if(transform.position.z >= 370 && enemy.transform.position.z >= 368)
		{
			if(p == true)		// Player가 이긴 경우 PlayerWinMenu Scene을 불러옴
			{
				SceneManager.LoadScene("PlayerWinMenu");
			}
			else		// Player가 진 경우 PlayerLoseMenu Scene을 불러옴
			{
				SceneManager.LoadScene("PlayerLoseMenu");
			}
		}

		if(rb.velocity.z <= 2)		// 출발하지 않는 경우 audioidle을 재생
		{
			this.audioidle.Play();
		}

		else if(rb.velocity.z > 2 && rb.velocity.z <= 15)  // 속도가 15이기 전까지는 audiolow를 재생
		{
			this.audiolow.Play();
		}

		else if(rb.velocity.z > 15 && rb.velocity.z <= 30)	// 속도가 30이기 전까지는 audiomid를 재생
		{
			this.audiomid.Play();
		}

		else if(rb.velocity.z > 30) // 속도가 30을 넘으면 audiohigh를 재생
		{
			this.audiohigh.Play();
		}
		
		m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);
        // 상하좌우 키를 이용하여 angle과 torque를 조절
		float angle = maxAngle * Input.GetAxis("Horizontal");
		float torque = maxTorque * Input.GetAxis("Vertical");

		float handBrake = Input.GetKey(KeyCode.X) ? brakeTorque : 0;		// X를 누르면 handbrake작동

		if(currentTime > 3)
		{
			tm.TimeUI.text = "Time : " + (currentTime - 3.0f);		// 현재 시간을 우측 상단에 표시
			sm.SpeedUI.text = "속도 : " + rb.velocity.z;		// 현재 속도를 우측 하단에 표시
			start.SetActive(false);		// 시작하면 3, 2, 1을 표시 했던 UI를 없앰

			foreach (WheelCollider wheel in m_Wheels)
			{
				// A simple car where front wheels steer while rear ones drive.
				if (wheel.transform.localPosition.z > 0)		// 전방의 wheel은 좌우로 움직임
					wheel.steerAngle = angle;

				if (wheel.transform.localPosition.z < 0)		// 후방의 wheel은 handbrake시 brake가 작동
				{
					wheel.brakeTorque = handBrake;
				}

				// 전륜구동, 후륜구동에 따른 torque를 받는 바퀴 설정

				if (wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive)		// 후륜구동
				{
					wheel.motorTorque = torque;
				}

				if (wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive)		// 전륜구동
				{
					wheel.motorTorque = torque;
				}

				// Update visual wheels if any.
				if (wheelShape) 
				{
					Quaternion q;
					Vector3 p;
					wheel.GetWorldPose (out p, out q);

					// Assume that the only child of the wheelcollider is the wheel shape.
					Transform shapeTransform = wheel.transform.GetChild (0);

                	if (wheel.name == "a0l" || wheel.name == "a1l" || wheel.name == "a2l")		// 움직임에 따른 바퀴 모양 변경
                	{
                   		shapeTransform.rotation = q * Quaternion.Euler(0, 180, 0);
                    	shapeTransform.position = p;
                	}
                	else
                	{
                    	shapeTransform.position = p;
                    	shapeTransform.rotation = q;
                	}
				}
			}
		}
	}
}
