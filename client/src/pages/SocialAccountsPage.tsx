import { useState } from 'react'
import { useForm } from 'react-hook-form'
import {
  useSocialAccounts,
  useCreateSocialAccount,
  useDeactivateSocialAccount,
  useValidateSocialAccount,
} from '@/hooks/use-social-accounts'
import { PlatformIcon } from '@/components/content/PlatformIcon'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { Plus, Shield, ShieldAlert, ShieldX, Loader2, Users, CheckCircle2 } from 'lucide-react'
import { toast } from 'sonner'
import { Platform } from '@/types'
import type { CreateSocialAccountDto } from '@/types'
import { format, differenceInDays } from 'date-fns'
import { cn } from '@/lib/utils'

export function SocialAccountsPage() {
  const { data: accounts, isLoading } = useSocialAccounts()
  const createAccount = useCreateSocialAccount()
  const deactivateAccount = useDeactivateSocialAccount()
  const validateAccount = useValidateSocialAccount()
  const [dialogOpen, setDialogOpen] = useState(false)

  const { register, handleSubmit, reset, setValue } = useForm<CreateSocialAccountDto>({
    defaultValues: { platform: 'Facebook' as const },
  })

  const onSubmit = async (data: CreateSocialAccountDto) => {
    try {
      await createAccount.mutateAsync(data)
      toast.success('Account created')
      setDialogOpen(false)
      reset()
    } catch {
      toast.error('Failed to create account')
    }
  }

  if (isLoading) {
    return (
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-48 rounded-xl bg-fm-surface-container" />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-5">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-fm-on-surface-variant">
          {accounts?.length ?? 0} connected accounts
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
              Connect Account
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
                Connect Social Account
              </DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Name</Label>
                <Input
                  {...register('name')}
                  placeholder="My Facebook Page"
                  className="h-10 bg-fm-surface-container text-fm-on-surface placeholder:text-fm-outline focus:bg-fm-surface-container-high"
                />
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Platform</Label>
                <Select defaultValue="Facebook" onValueChange={(v: string) => setValue('platform', v as CreateSocialAccountDto['platform'])}>
                  <SelectTrigger className="h-10 bg-fm-surface-container text-fm-on-surface">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent className="bg-fm-surface-container-highest">
                    {Object.values(Platform).map((p) => (
                      <SelectItem key={p} value={p}>{p}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">External ID</Label>
                <Input
                  {...register('externalId')}
                  placeholder="Page/Account ID"
                  className="h-10 bg-fm-surface-container text-fm-on-surface placeholder:text-fm-outline focus:bg-fm-surface-container-high"
                />
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-medium text-fm-on-surface-variant">Access Token</Label>
                <Input
                  type="password"
                  {...register('accessToken')}
                  placeholder="Access token"
                  className="h-10 bg-fm-surface-container text-fm-on-surface placeholder:text-fm-outline focus:bg-fm-surface-container-high"
                />
              </div>
              <button
                type="submit"
                disabled={createAccount.isPending}
                className="flex h-10 w-full items-center justify-center rounded-lg text-sm font-semibold text-fm-on-background transition-all disabled:cursor-not-allowed disabled:opacity-50"
                style={{
                  background: 'linear-gradient(135deg, var(--fm-primary-dim), var(--fm-primary))',
                }}
              >
                {createAccount.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Create Account'}
              </button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Account cards grid */}
      {accounts && accounts.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {accounts.map((account) => {
            const daysUntilExpiry = account.tokenExpiresAt
              ? differenceInDays(new Date(account.tokenExpiresAt), new Date())
              : null

            return (
              <div
                key={account.id}
                className="group rounded-xl bg-fm-surface-container p-5 transition-colors hover:bg-fm-surface-container-high"
              >
                {/* Header: platform icon + status indicator */}
                <div className="flex items-center justify-between">
                  <PlatformIcon platform={account.platform} showLabel className="h-5 w-5" />
                  <div className="flex items-center gap-2">
                    <span
                      className={cn(
                        'inline-block h-2 w-2 rounded-full',
                        account.isActive ? 'bg-emerald-400' : 'bg-fm-outline',
                      )}
                    />
                    <span className={cn(
                      'text-xs font-medium',
                      account.isActive ? 'text-emerald-400' : 'text-fm-on-surface-variant',
                    )}>
                      {account.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                </div>

                {/* Account name */}
                <h3 className="mt-3 font-display text-base font-bold text-fm-on-surface">
                  {account.name}
                </h3>

                {/* External ID */}
                <p className="mt-1 font-mono text-xs text-fm-on-surface-variant">
                  ID: {account.externalId}
                </p>

                {/* Token expiry */}
                {account.tokenExpiresAt && (
                  <div className="mt-3 flex items-center gap-1.5 text-xs">
                    {daysUntilExpiry !== null && daysUntilExpiry <= 0 ? (
                      <ShieldX className="h-3.5 w-3.5 text-fm-error" />
                    ) : daysUntilExpiry !== null && daysUntilExpiry < 7 ? (
                      <ShieldAlert className="h-3.5 w-3.5 text-amber-400" />
                    ) : (
                      <CheckCircle2 className="h-3.5 w-3.5 text-emerald-400" />
                    )}
                    <span className={cn(
                      'text-xs',
                      daysUntilExpiry !== null && daysUntilExpiry <= 0
                        ? 'text-fm-error'
                        : daysUntilExpiry !== null && daysUntilExpiry < 7
                          ? 'text-amber-400'
                          : 'text-fm-on-surface-variant',
                    )}>
                      {daysUntilExpiry !== null && daysUntilExpiry <= 0
                        ? 'Token expired'
                        : `Expires ${format(new Date(account.tokenExpiresAt), 'MMM d, yyyy')}`}
                    </span>
                  </div>
                )}

                {/* Actions */}
                <div className="mt-4 flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 border-fm-outline-variant/30 bg-transparent text-xs text-fm-on-surface-variant hover:bg-fm-surface-bright hover:text-fm-on-surface"
                    onClick={async () => {
                      const res = await validateAccount.mutateAsync(account.id)
                      if (res.isValid) toast.success('Token is valid')
                      else toast.error(res.errorMessage ?? 'Token is invalid')
                    }}
                  >
                    <Shield className="mr-1 h-3 w-3" />
                    Validate
                  </Button>
                  {account.isActive && (
                    <Button
                      size="sm"
                      variant="ghost"
                      className="h-7 text-xs text-fm-error hover:bg-fm-error/10 hover:text-fm-error"
                      onClick={async () => {
                        await deactivateAccount.mutateAsync(account.id)
                        toast.success('Account deactivated')
                      }}
                    >
                      Deactivate
                    </Button>
                  )}
                </div>
              </div>
            )
          })}

          {/* Add card (dashed) */}
          <button
            onClick={() => setDialogOpen(true)}
            className="flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-fm-outline-variant/30 bg-transparent p-5 text-fm-on-surface-variant transition-colors hover:border-fm-primary/40 hover:bg-fm-surface-container hover:text-fm-primary"
          >
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-fm-surface-container-high">
              <Plus className="h-5 w-5" />
            </div>
            <span className="text-sm font-medium">Connect new account</span>
          </button>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-4 pt-16">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-fm-surface-container">
            <Users className="h-8 w-8 text-fm-on-surface-variant" />
          </div>
          <p className="text-fm-on-surface-variant">No social accounts connected yet.</p>
          <button
            onClick={() => setDialogOpen(true)}
            className="inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold text-fm-on-background transition-all hover:glow-primary"
            style={{
              background: 'linear-gradient(135deg, var(--fm-primary-dim), var(--fm-primary))',
            }}
          >
            <Plus className="h-4 w-4" />
            Connect your first account
          </button>
        </div>
      )}
    </div>
  )
}
