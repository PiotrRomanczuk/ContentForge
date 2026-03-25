export interface ScheduleConfigDto {
  id: string
  botRegistrationId: string
  botName: string
  socialAccountId: string
  accountName: string
  cronExpression: string
  isActive: boolean
  preferredContentType: string
  createdAt: string
}

export interface CreateScheduleDto {
  botRegistrationId: string
  socialAccountId: string
  cronExpression: string
  preferredContentType?: string
  overrideConfig?: Record<string, string>
}

export interface UpdateScheduleDto {
  cronExpression?: string
  isActive?: boolean
  preferredContentType?: string
}

export interface JobStatusDto {
  jobId: string
  jobName: string
  cronExpression: string
  nextExecution: string | null
  lastExecution: string | null
}
