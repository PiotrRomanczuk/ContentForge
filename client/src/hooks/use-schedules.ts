import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { schedulesApi } from '@/api/schedules'
import { queryKeys } from '@/lib/query-keys'
import type { CreateScheduleDto, UpdateScheduleDto } from '@/types'

export function useSchedules(active?: boolean) {
  return useQuery({
    queryKey: queryKeys.schedules.list(active),
    queryFn: () => schedulesApi.getAll(active),
  })
}

export function useScheduleById(id: string) {
  return useQuery({
    queryKey: queryKeys.schedules.detail(id),
    queryFn: () => schedulesApi.getById(id),
    enabled: !!id,
  })
}

export function useJobStatuses() {
  return useQuery({
    queryKey: queryKeys.schedules.jobs(),
    queryFn: () => schedulesApi.getJobs(),
    refetchInterval: 30_000,
  })
}

export function useCreateSchedule() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateScheduleDto) => schedulesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.schedules.all })
    },
  })
}

export function useUpdateSchedule() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateScheduleDto }) =>
      schedulesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.schedules.all })
    },
  })
}

export function useDeactivateSchedule() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => schedulesApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.schedules.all })
    },
  })
}
