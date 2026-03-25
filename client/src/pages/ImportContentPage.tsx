import { useState } from 'react'
import { useForm, useFieldArray } from 'react-hook-form'
import { useImportContent } from '@/hooks/use-content'
import { useBots } from '@/hooks/use-bots'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Badge } from '@/components/ui/badge'
import { Plus, Trash2, Upload, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { ContentType } from '@/types'
import type { ImportContentRequest, ContentBatchResultDto } from '@/types'

interface FormValues {
  items: {
    botName: string
    category: string
    contentType: string
    textContent: string
    scheduledAt: string
  }[]
}

export function ImportContentPage() {
  const { data: bots } = useBots()
  const importMutation = useImportContent()
  const [result, setResult] = useState<ContentBatchResultDto | null>(null)
  const [jsonInput, setJsonInput] = useState('')

  const { register, control, handleSubmit, reset } = useForm<FormValues>({
    defaultValues: {
      items: [{ botName: '', category: '', contentType: 'Image', textContent: '', scheduledAt: '' }],
    },
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'items' })

  const onFormSubmit = async (data: FormValues) => {
    const request: ImportContentRequest = {
      items: data.items.map((item) => ({
        botName: item.botName,
        category: item.category,
        contentType: item.contentType as keyof typeof ContentType,
        textContent: item.textContent,
        scheduledAt: item.scheduledAt || undefined,
      })),
    }

    try {
      const res = await importMutation.mutateAsync(request)
      setResult(res)
      toast.success(`Imported ${res.succeeded} of ${res.totalGenerated} items`)
      reset()
    } catch {
      toast.error('Import failed')
    }
  }

  const onJsonSubmit = async () => {
    try {
      const parsed = JSON.parse(jsonInput) as ImportContentRequest
      const res = await importMutation.mutateAsync(parsed)
      setResult(res)
      toast.success(`Imported ${res.succeeded} of ${res.totalGenerated} items`)
      setJsonInput('')
    } catch {
      toast.error('Invalid JSON or import failed')
    }
  }

  return (
    <div className="space-y-6">
      <Tabs defaultValue="form">
        <TabsList className="border-none bg-fm-surface-container" variant="line">
          <TabsTrigger
            value="form"
            className="text-fm-on-surface-variant data-[state=active]:text-fm-primary after:bg-fm-primary"
          >
            Form Import
          </TabsTrigger>
          <TabsTrigger
            value="json"
            className="text-fm-on-surface-variant data-[state=active]:text-fm-primary after:bg-fm-primary"
          >
            JSON Batch
          </TabsTrigger>
        </TabsList>

        <TabsContent value="form" className="mt-5">
          <form onSubmit={handleSubmit(onFormSubmit)}>
            <div className="space-y-4">
              {fields.map((field, index) => (
                <div key={field.id} className="relative rounded-xl bg-fm-surface-container p-5">
                  {fields.length > 1 && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      onClick={() => remove(index)}
                      className="absolute right-3 top-3 h-7 w-7 text-fm-error/70 hover:bg-fm-error/10 hover:text-fm-error"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}

                  <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    <div className="space-y-2">
                      <Label className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
                        Bot
                      </Label>
                      <Select
                        onValueChange={(val: string) => {
                          const event = { target: { value: val, name: `items.${index}.botName` } }
                          register(`items.${index}.botName`).onChange(event as never)
                        }}
                      >
                        <SelectTrigger className="border-none bg-fm-surface-container-highest text-fm-on-surface">
                          <SelectValue placeholder="Select bot" />
                        </SelectTrigger>
                        <SelectContent className="border-fm-outline-variant/15 bg-fm-surface-container-highest shadow-ambient backdrop-blur-xl">
                          {bots?.map((bot) => (
                            <SelectItem key={bot.name} value={bot.name}>
                              {bot.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
                        Category
                      </Label>
                      <Input
                        {...register(`items.${index}.category`)}
                        placeholder="e.g. Language Learning"
                        className="border-none bg-fm-surface-container-highest text-fm-on-surface placeholder:text-fm-on-surface-variant/50"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
                        Content Type
                      </Label>
                      <Select
                        defaultValue="Image"
                        onValueChange={(val: string) => {
                          const event = { target: { value: val, name: `items.${index}.contentType` } }
                          register(`items.${index}.contentType`).onChange(event as never)
                        }}
                      >
                        <SelectTrigger className="border-none bg-fm-surface-container-highest text-fm-on-surface">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent className="border-fm-outline-variant/15 bg-fm-surface-container-highest shadow-ambient backdrop-blur-xl">
                          {Object.values(ContentType).map((t) => (
                            <SelectItem key={t} value={t}>{t}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2 sm:col-span-2">
                      <Label className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
                        Text Content
                      </Label>
                      <Textarea
                        {...register(`items.${index}.textContent`)}
                        rows={3}
                        placeholder="Content text..."
                        className="border-none bg-fm-surface-container-highest text-fm-on-surface placeholder:text-fm-on-surface-variant/50"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
                        Scheduled At
                      </Label>
                      <Input
                        type="datetime-local"
                        {...register(`items.${index}.scheduledAt`)}
                        className="border-none bg-fm-surface-container-highest text-fm-on-surface"
                      />
                    </div>
                  </div>
                </div>
              ))}

              <div className="flex gap-3">
                <Button
                  type="button"
                  variant="ghost"
                  onClick={() =>
                    append({ botName: '', category: '', contentType: 'Image', textContent: '', scheduledAt: '' })
                  }
                  className="gap-1 text-fm-on-surface-variant hover:text-fm-on-surface"
                >
                  <Plus className="h-4 w-4" />
                  Add Item
                </Button>
                <Button
                  type="submit"
                  disabled={importMutation.isPending}
                  className="gap-1 bg-gradient-to-br from-fm-primary-dim to-fm-primary text-fm-background hover:glow-primary"
                >
                  {importMutation.isPending ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Upload className="h-4 w-4" />
                  )}
                  Import {fields.length} Item(s)
                </Button>
              </div>
            </div>
          </form>
        </TabsContent>

        <TabsContent value="json" className="mt-5 space-y-4">
          <div className="rounded-xl bg-fm-surface-container p-5">
            <Label className="mb-2 block text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
              Paste JSON
            </Label>
            <Textarea
              value={jsonInput}
              onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setJsonInput(e.target.value)}
              rows={12}
              className="border-none bg-fm-surface-container-highest font-mono text-xs text-fm-on-surface placeholder:text-fm-on-surface-variant/50"
              placeholder={`{\n  "items": [\n    {\n      "botName": "EnglishFactsBot",\n      "category": "Language Learning",\n      "contentType": "Image",\n      "textContent": "Did you know..."\n    }\n  ]\n}`}
            />
          </div>
          <Button
            onClick={onJsonSubmit}
            disabled={importMutation.isPending || !jsonInput.trim()}
            className="gap-1 bg-gradient-to-br from-fm-primary-dim to-fm-primary text-fm-background hover:glow-primary"
          >
            {importMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />}
            Import JSON
          </Button>
        </TabsContent>
      </Tabs>

      {/* Results */}
      {result && (
        <div className="rounded-xl bg-fm-surface-container p-5">
          <h3 className="mb-3 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
            Import Results
          </h3>
          <div className="space-y-3">
            <div className="flex gap-3">
              <Badge variant="secondary" className="bg-fm-surface-container-high text-fm-on-surface-variant">
                {result.totalGenerated} total
              </Badge>
              <Badge className="bg-emerald-500/10 text-emerald-400">{result.succeeded} succeeded</Badge>
              {result.failed > 0 && (
                <Badge className="bg-fm-error/10 text-fm-error">{result.failed} failed</Badge>
              )}
            </div>
            {result.errors.length > 0 && (
              <div className="rounded-lg bg-fm-error/5 p-3">
                <p className="text-xs font-medium text-fm-error">Errors:</p>
                <ul className="mt-1 list-inside list-disc text-xs text-fm-error/80">
                  {result.errors.map((err, i) => (
                    <li key={i}>{err}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
