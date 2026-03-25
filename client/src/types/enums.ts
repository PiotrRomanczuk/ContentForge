export const ContentStatus = {
  Draft: 'Draft',
  Generated: 'Generated',
  Rendered: 'Rendered',
  Queued: 'Queued',
  Publishing: 'Publishing',
  Published: 'Published',
  Failed: 'Failed',
} as const
export type ContentStatus = (typeof ContentStatus)[keyof typeof ContentStatus]

export const CONTENT_STATUS_ORDER: ContentStatus[] = [
  ContentStatus.Draft,
  ContentStatus.Generated,
  ContentStatus.Rendered,
  ContentStatus.Queued,
  ContentStatus.Publishing,
  ContentStatus.Published,
  ContentStatus.Failed,
]

export const ContentType = {
  Image: 'Image',
  Carousel: 'Carousel',
  Video: 'Video',
  Story: 'Story',
  Text: 'Text',
} as const
export type ContentType = (typeof ContentType)[keyof typeof ContentType]

export const Platform = {
  Facebook: 'Facebook',
  Instagram: 'Instagram',
  TikTok: 'TikTok',
  YouTube: 'YouTube',
} as const
export type Platform = (typeof Platform)[keyof typeof Platform]
