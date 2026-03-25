import { api } from './client'
import type {
  ScheduleConfigDto,
  CreateScheduleDto,
  UpdateScheduleDto,
  JobStatusDto,
} from '@/types'

export const schedulesApi = {
  getAll: (active?: boolean) =>
    api
      .get('api/schedules', {
        searchParams: active !== undefined ? { active: String(active) } : {},
      })
      .json<ScheduleConfigDto[]>(),

  getById: (id: string) => api.get(`api/schedules/${id}`).json<ScheduleConfigDto>(),

  create: (data: CreateScheduleDto) =>
    api.post('api/schedules', { json: data }).json<ScheduleConfigDto>(),

  update: (id: string, data: UpdateScheduleDto) =>
    api.put(`api/schedules/${id}`, { json: data }).json<ScheduleConfigDto>(),

  deactivate: (id: string) => api.delete(`api/schedules/${id}`),

  getJobs: () => api.get('api/schedules/jobs').json<JobStatusDto[]>(),
}
