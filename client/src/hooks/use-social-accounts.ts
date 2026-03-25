import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { socialAccountsApi } from '@/api/social-accounts'
import { queryKeys } from '@/lib/query-keys'
import type { CreateSocialAccountDto, UpdateSocialAccountDto, Platform } from '@/types'

export function useSocialAccounts(platform?: Platform) {
  return useQuery({
    queryKey: queryKeys.socialAccounts.list(platform),
    queryFn: () => socialAccountsApi.getAll(platform),
  })
}

export function useSocialAccountById(id: string) {
  return useQuery({
    queryKey: queryKeys.socialAccounts.detail(id),
    queryFn: () => socialAccountsApi.getById(id),
    enabled: !!id,
  })
}

export function useCreateSocialAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateSocialAccountDto) => socialAccountsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.socialAccounts.all })
    },
  })
}

export function useUpdateSocialAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSocialAccountDto }) =>
      socialAccountsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.socialAccounts.all })
    },
  })
}

export function useDeactivateSocialAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => socialAccountsApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.socialAccounts.all })
    },
  })
}

export function useValidateSocialAccount() {
  return useMutation({
    mutationFn: (id: string) => socialAccountsApi.validate(id),
  })
}
