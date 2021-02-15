// MathLibrary.h - Contains declarations of math functions
#pragma once

#include "heatmap.h"

__declspec(dllexport) heatmap_t* AllocHeatmap(unsigned w, unsigned h);
__declspec(dllexport) void AddPointToHeatmap(heatmap_t* h, unsigned x, unsigned y);
__declspec(dllexport) void AddPointToHeatmapWithStamp(heatmap_t* h, unsigned x, unsigned y, const heatmap_stamp_t* stamp);
__declspec(dllexport) void WriteHeatmap(const heatmap_t* h, unsigned char* colorbuf);
__declspec(dllexport) void FreeHeatmap(heatmap_t* h);

__declspec(dllexport) heatmap_stamp_t* CreateStamp(unsigned r);
__declspec(dllexport) void FreeStamp(heatmap_stamp_t* s);


