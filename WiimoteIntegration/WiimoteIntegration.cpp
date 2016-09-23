// WiimoteIntegration.cpp : Defines the entry point for the console application.
//

#include "WiimoteIntegration.h"
using namespace cv;
using namespace std;

#define WIIMOTE_UPDATE_FREQ 95.0f

#define GYRO_FACTOR (15.0f)

#define NORM_FACTOR (16.0f) // always 1 + GYRO_FACTOR



float test() {
	return 42.0f;
}

volatile float RxEst, RyEst, RzEst, RxAcc, RyAcc, RzAcc;
volatile float Axz, Axy, Ayz, RateAxz, RateAyz;
volatile vec3 m_out;


void init_model() {
	RxEst, RyEst, RzEst, RxAcc, RyAcc, RzAcc, Axz, Axy, Ayz, RateAxz = 0;
	m_out = (vec3)malloc(sizeof(vec3));

}

void update_model(float accel_x, float accel_y, float accel_z, float gyro_roll, float gyro_pitch, float gyro_yaw) {

	Axz = atan2f(RxEst, RzEst);
	float RateAxzAvg = (RateAxz + gyro_pitch) / 2.0f;
	float Axz_1 = Axz + RateAxzAvg / WIIMOTE_UPDATE_FREQ;

	Ayz = atan2f(RyEst, RzEst);
	float RateAyzAvg = (RateAyz + gyro_roll) / 2.0f;
	float Ayz_1 = Ayz + RateAyzAvg / WIIMOTE_UPDATE_FREQ;

	float _cosfAxz_1 = cosf(Axz_1);
	float _tanfAyz_1 = tanf(Ayz_1);
	float _cosAxztanAxz = _cosfAxz_1 * _tanfAyz_1;
	float RxGyro = sinf(Axz_1) / sqrtf(1 + _cosAxztanAxz * _cosAxztanAxz);

	float _cosfAyz_1 = cosf(Ayz_1);
	float _tanfAxz_1 = tanf(Axz_1);
	float _cosAyztanAxz = _cosfAyz_1 * _tanfAxz_1;
	float RyGyro = sinf(Ayz_1) / sqrtf(1 + _cosAyztanAxz * _cosAyztanAxz);
	float RzGyro = copysignf(RzEst, sqrtf(1.0f - RxGyro * RxGyro - RyGyro * RyGyro));

	RxEst = (accel_x + RxGyro * GYRO_FACTOR) / NORM_FACTOR;
	RyEst = (accel_y + RyGyro * GYRO_FACTOR) / NORM_FACTOR;
	RzEst = (accel_z + RzGyro * GYRO_FACTOR) / NORM_FACTOR;
}

void dealloc() {
	if(m_out!=NULL)
	free(m_out);
}

float get_model_x() {
	return RxEst;
}

float get_model_y() {
	return RyEst;
}

float get_model_z() {
	return RzEst;
}