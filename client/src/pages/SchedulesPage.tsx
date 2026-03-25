import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useSchedules, useJobStatuses, useCreateSchedule, useDeactivateSchedule } from '@/hooks/use-schedules'
import { useBots } from '@/hooks/use-bots'
import { useSocialAccounts } from '@/hooks/use-social-accounts'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Skeleton } from '@/components/ui/skeleton'
import { Plus, Clock, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { ContentType } from '@/types'
import type { CreateScheduleDto } from '@/types'
import { format } from 'date-fns'
import { cn } from '@/lib/utils'

export function SchedulesPage() {
  const { data: schedules, isLoading } = useSchedules()
  const { data: jobs } = useJobStatuses()
  const { data: bots } = useBots()
  const { data: accounts } = useSocialAccounts()
  const createSchedule = useCreateSchedule()
  const deactivateSchedule = useDeactivateSchedule()
  const [dialogOpen, setDialogOpen] = useState(false)

  const { register, handleSubmit, reset, setValue } = useForm<CreateScheduleDto>()

  const onSubmit = async (data: CreateScheduleDto) => {
    try {
      await createSchedule.mutateAsync(data)
      toast.success('Schedule created')
      setDialogOpen(false)
      reset()
    } catch {
      toast.error('Failed to create schedule')
    }
  }

  if (isLoading) {
    return <Skeleton className="h-64 rounded-xl bg-fm-surface-container" />
  }

  return (
    <div className="space-y-8">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-fm-on-surface-variant">
          {schedules?.length ?? 0} publishing schedules
        </p>
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <button
              className="inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold text-fm-on-background transition-all hover:glow-primary"
              style={{
                background: 'linear-gradient(135deg, var(--fm-primary-dim), var(--fm-primary))',
              }}
            >
              <Plus className="h-4 w-4" />
              Create Schedule
            </button>
          </DialogTrigger>
          <DialogContent
            className="border-ghost shadow-ambient bg-fm-surface-container-highest"
            style={{
              backdropFilter: 'blur(24px)',
              WebkitBackdropFilter: 'blur(24px)',
            }}
          >
            <DialogHeader>
              <DialogTitle className="font-display text-fm-on-background">
                Create Schedule
              </DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Bot</Label>
                <Select onValueChange={(v: string) => setValue('botRegistrationId', v)}>
                  <SelectTrigger className="h-10 bg-fm-surface-container text-fm-on-surface">
                    <SelectValue placeholder="Select bot" />
                  </SelectTrigger>
                  <SelectContent className="bg-fm-surface-container-highest">
                    {bots?.map((bot) => (
                      <SelectItem key={bot.name} value={bot.name}>{bot.name}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Social Account</Label>
                <Select onValueChange={(v: string) => setValue('socialAccountId', v)}>
                  <SelectTrigger className="h-10 bg-fm-surface-container text-fm-on-surface">
                    <SelectValue placeholder="Select account" />
                  </SelectTrigger>
                  <SelectContent className="bg-fm-surface-container-highest">
                    {accounts?.map((acc) => (
                      <SelectItem key={acc.id} value={acc.id}>{acc.name} ({acc.platform})</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Cron Expression</Label>
                <Input
                  {...register('cronExpression')}
                  placeholder="0 9 * * *"
                  className="h-10 bg-fm-surface-container text-fm-on-surface placeholder:text-fm-outline focus:bg-fm-surface-container-high"
                />
                <p className="text-[11px] text-fm-on-surface-variant/70">
                  e.g. &quot;0 9 * * *&quot; = every day at 9 AM
                </p>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Content Type</Label>
                <Select onValueChange={(v: string) => setValue('preferredContentType', v)}>
                  <SelectTrigger className="h-10 bg-fm-surface-container text-fm-on-surface">
                    <SelectValue placeholder="Any type" />
                  </SelectTrigger>
                  <SelectContent className="bg-fm-surface-container-highest">
                    {Object.values(ContentType).map((t) => (
                      <SelectItem key={t} value={t}>{t}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <button
                type="submit"
                disabled={createSchedule.isPending}
                className="flex h-10 w-full items-center justify-center rounded-lg text-sm font-semibold text-fm-on-background transition-all disabled:cursor-not-allowed disabled:opacity-50"
                style={{
                  background: 'linear-gradient(135deg, var(--fm-primary-dim), var(--fm-primary))',
                }}
              >
                {createSchedule.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Create Schedule'}
              </button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Section 1: Publishing Schedules */}
      <section className="space-y-3">
        <h2 className="font-display text-lg font-bold text-fm-on-background">
          Publishing Schedules
        </h2>

        {schedules && schedules.length > 0 ? (
          <div className="overflow-hidden rounded-xl bg-fm-surface-container">
            <Table>
              <TableHeader>
                <TableRow className="border-b border-fm-outline-variant/15 hover:bg-transparent">
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Bot</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Account</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Cron</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Type</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Status</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {schedules.map((schedule) => (
                  <TableRow
                    key={schedule.id}
                    className="border-b border-fm-outline-variant/10 bg-fm-surface transition-colors hover:bg-fm-surface-container"
                  >
                    <TableCell className="font-medium text-fm-on-surface">
                      {schedule.botName}
                    </TableCell>
                    <TableCell className="text-sm text-fm-on-surface-variant">
                      {schedule.accountName}
                    </TableCell>
                    <TableCell>
                      <code className="rounded bg-fm-surface-container-high px-1.5 py-0.5 font-mono text-xs text-fm-on-surface-variant">
                        {schedule.cronExpression}
                      </code>
                    </TableCell>
                    <TableCell className="text-sm text-fm-on-surface-variant">
                      {schedule.preferredContentType}
                    </TableCell>
                    <TableCell>
                      <span
                        className={cn(
                          'inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium',
                          schedule.isActive
                            ? 'bg-emerald-500/10 text-emerald-400'
                            : 'bg-fm-surface-container-high text-fm-on-surface-variant',
                        )}
                      >
                        <span
                          className={cn(
                            'inline-block h-1.5 w-1.5 rounded-full',
                            schedule.isActive ? 'bg-emerald-400' : 'bg-fm-outline',
                          )}
                        />
                        {schedule.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </TableCell>
                    <TableCell>
                      {schedule.isActive && (
                        <Button
                          size="sm"
                          variant="ghost"
                          className="h-7 text-xs text-fm-on-surface-variant hover:bg-fm-surface-bright hover:text-fm-on-surface"
                          onClick={async () => {
                            await deactivateSchedule.mutateAsync(schedule.id)
                            toast.success('Schedule deactivated')
                          }}
                        >
                          Deactivate
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-4 rounded-xl bg-fm-surface-container py-12">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-fm-surface-container-high">
              <Clock className="h-6 w-6 text-fm-on-surface-variant" />
            </div>
            <p className="text-sm text-fm-on-surface-variant">No schedules configured yet.</p>
          </div>
        )}
      </section>

      {/* Section 2: Hangfire Jobs */}
      {jobs && jobs.length > 0 && (
        <section className="space-y-3">
          <h2 className="font-display text-base font-semibold text-fm-on-surface-variant">
            Hangfire Jobs
          </h2>

          <div className="overflow-hidden rounded-xl bg-fm-surface-container-low">
            <Table>
              <TableHeader>
                <TableRow className="border-b border-fm-outline-variant/10 hover:bg-transparent">
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant/70">Job</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant/70">Cron</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant/70">Next Run</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant/70">Last Run</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {jobs.map((job) => (
                  <TableRow
                    key={job.jobId}
                    className="border-b border-fm-outline-variant/8 hover:bg-fm-surface-container-low"
                  >
                    <TableCell className="text-sm font-medium text-fm-on-surface-variant">
                      {job.jobName}
                    </TableCell>
                    <TableCell>
                      <code className="font-mono text-xs text-fm-on-surface-variant/70">
                        {job.cronExpression}
                      </code>
                    </TableCell>
                    <TableCell className="text-xs text-fm-on-surface-variant/70">
                      {job.nextExecution ? format(new Date(job.nextExecution), 'PPp') : '\u2014'}
                    </TableCell>
                    <TableCell className="text-xs text-fm-on-surface-variant/70">
                      {job.lastExecution ? format(new Date(job.lastExecution), 'PPp') : '\u2014'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </section>
      )}
    </div>
  )
}
