#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <string.h>
#include <sys/ioctl.h>
#include <linux/perf_event.h>
#include <asm/unistd.h>

static long
perf_event_open(struct perf_event_attr *hw_event, pid_t pid,
                int cpu, int group_fd, unsigned long flags)
{
    int ret;

    ret = syscall(__NR_perf_event_open, hw_event, pid, cpu,
                    group_fd, flags);
    return ret;
}

int perf_event_counter(int32_t type, int32_t config, int group_fd) {
    struct perf_event_attr pe;
    memset(&pe, 0, sizeof(struct perf_event_attr));
    pe.type = type;
    pe.size = sizeof(struct perf_event_attr);
    pe.config = config;
    pe.disabled = 1;
    pe.exclude_kernel = 1;
    pe.exclude_hv = 1;

    int fd = perf_event_open(&pe, 0, -1, group_fd, 0);
    if (fd == -1) {
        ioctl(fd, PERF_EVENT_IOC_RESET, 0);
    }
    return fd;
}

typedef struct {
    int leader,
        instructions,
        cycles,
        branches,
        branch_misses,
        cpu_clock,
        task_clock,
        page_faults,
        context_switches,
        cpu_migrations;
} Handles;

Handles* pinvoke_init() {
    Handles* handles = malloc(sizeof(Handles));
    memset(handles, 0, sizeof(Handles));
    handles->instructions = perf_event_counter(PERF_TYPE_HARDWARE, PERF_COUNT_HW_INSTRUCTIONS, -1);
    handles->leader = handles->instructions;
    handles->cycles = perf_event_counter(PERF_TYPE_HARDWARE, PERF_COUNT_HW_CPU_CYCLES, handles->leader);
    handles->branches = perf_event_counter(PERF_TYPE_HARDWARE, PERF_COUNT_HW_BRANCH_INSTRUCTIONS, handles->leader);
    handles->branch_misses = perf_event_counter(PERF_TYPE_HARDWARE, PERF_COUNT_HW_BRANCH_MISSES, handles->leader);

    handles->cpu_clock = perf_event_counter(PERF_TYPE_SOFTWARE, PERF_COUNT_SW_CPU_CLOCK, handles->leader);
    handles->task_clock = perf_event_counter(PERF_TYPE_SOFTWARE, PERF_COUNT_SW_TASK_CLOCK, handles->leader);
    handles->page_faults = perf_event_counter(PERF_TYPE_SOFTWARE, PERF_COUNT_SW_PAGE_FAULTS, handles->leader);
    handles->context_switches = perf_event_counter(PERF_TYPE_SOFTWARE, PERF_COUNT_SW_CONTEXT_SWITCHES, handles->leader);
    handles->cpu_migrations = perf_event_counter(PERF_TYPE_SOFTWARE, PERF_COUNT_SW_CPU_MIGRATIONS, handles->leader);

    ioctl(handles->leader, PERF_EVENT_IOC_RESET, PERF_IOC_FLAG_GROUP);
    
    return handles;
}

void pinvoke_start_perf(Handles* handles) {
    ioctl(handles->leader, PERF_EVENT_IOC_ENABLE, PERF_IOC_FLAG_GROUP);
}

void pinvoke_stop_perf(Handles* handles) {
    ioctl(handles->leader, PERF_EVENT_IOC_DISABLE, PERF_IOC_FLAG_GROUP);
}

int pinvoke_num_counters() {
    return sizeof(Handles) / sizeof(int) - 1;
}

int64_t pinvoke_read_counter(Handles* handles, int offset) {
    int64_t count;
    int* ptr = handles;
    int fd = *(ptr + offset + 1);
    read(fd, &count, sizeof(int64_t));
    return count;
}

char* pinvoke_get_counter_name(int offset) {
    switch(offset) {
        case 0: return "Instructions";
        case 1: return "Cycles";
        case 2: return "Branches";
        case 3: return "BranchMisses";
        case 4: return "CpuClock";
        case 5: return "TaskClock";
        case 6: return "PageFaults";
        case 7: return "ContextSwitches";
        case 8: return "CpuMigrations";
        default: return 0;
    }
}

void pinvoke_close(Handles* handles) {
    for(int i = 0; i < pinvoke_num_counters(); i++) {
        int* ptr = handles;
        close(ptr[i + 1]);
    }
    free(handles);
}
