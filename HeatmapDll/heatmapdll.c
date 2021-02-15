#include "heatmapdll.h"
#include <stdlib.h>

heatmap_t* AllocHeatmap(unsigned w, unsigned h)
{
    return heatmap_new(w, h);
}

void AddPointToHeatmap(heatmap_t* h, unsigned x, unsigned y)
{
    heatmap_add_point(h, x, y);
}

void AddPointToHeatmapWithStamp(heatmap_t* h, unsigned x, unsigned y, const heatmap_stamp_t* stamp)
{
    heatmap_add_point_with_stamp(h, x, y, stamp);
}

void WriteHeatmap(const heatmap_t* h, unsigned char* colorbuf)
{
    heatmap_render_default_to(h, colorbuf);
}

void FreeHeatmap(heatmap_t* h)
{
    heatmap_free(h);
}

heatmap_stamp_t* CreateStamp(unsigned r)
{
    return heatmap_stamp_gen(r);
}

void FreeStamp(heatmap_stamp_t* s)
{
    heatmap_stamp_free(s);
}
