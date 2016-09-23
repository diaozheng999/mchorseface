#pragma once


#include "stdafx.h"
#include <math.h>
#include "opencv2/video/tracking.hpp"

#ifdef __cplusplus
extern "C" {
#endif

	struct s_vec3 {
		float x;
		float y;
		float z;
	};

	typedef struct s_vec3* vec3;

	/**
	 * A testing function that returns a float value (5). Used to test that
	 * DLL function calls succeed
	 */
	__declspec(dllexport) float test();

	__declspec(dllexport) void update_model(float accel_x, float accel_y, float accel_z, float gyro_roll, float gyro_pitch, float gyro_yaw);

	__declspec(dllexport) void dealloc();

	__declspec(dllexport) float get_model_x();
	__declspec(dllexport) float get_model_y();
	__declspec(dllexport) float get_model_z();

	__declspec(dllexport) void init_model();

	//__declspec(dllexport) void set_gyro_data(float roll, float pitch, float yaw);
	
	//__declspec(dllexport) void set_ir_data(unsigned char* data);

#ifdef __cplusplus
}
#endif