import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  flexRender,
  createColumnHelper,
} from '@tanstack/react-table'
import type { SortingState } from '@tanstack/react-table'
import { useState } from 'react'
import { format } from 'date-fns'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Button } from '@/components/ui/button'
import { StatusBadge } from './StatusBadge'
import { ContentTypeBadge } from './ContentTypeBadge'
import { ChevronLeft, ChevronRight, ExternalLink } from 'lucide-react'
import type { ContentItemDto } from '@/types'

const columnHelper = createColumnHelper<ContentItemDto>()

const columns = [
  columnHelper.accessor('status', {
    header: 'Status',
    cell: (info) => <StatusBadge status={info.getValue()} />,
    size: 120,
  }),
  columnHelper.accessor('botName', {
    header: 'Bot',
    cell: (info) => (
      <div>
        <p className="text-sm font-medium text-fm-on-surface">{info.getValue()}</p>
        <p className="text-xs text-fm-on-surface-variant">{info.row.original.category}</p>
      </div>
    ),
    size: 140,
  }),
  columnHelper.accessor('contentType', {
    header: 'Type',
    cell: (info) => <ContentTypeBadge contentType={info.getValue()} />,
    size: 100,
  }),
  columnHelper.accessor('textContent', {
    header: 'Content',
    cell: (info) => (
      <p className="max-w-[300px] truncate text-sm text-fm-on-surface-variant">{info.getValue()}</p>
    ),
  }),
  columnHelper.accessor('scheduledAt', {
    header: 'Scheduled',
    cell: (info) => {
      const val = info.getValue()
      return val ? (
        <span className="text-sm text-fm-on-surface-variant">
          {format(new Date(val), 'MMM d, HH:mm')}
        </span>
      ) : (
        <span className="text-sm text-fm-outline">{'\u2014'}</span>
      )
    },
    size: 140,
  }),
  columnHelper.accessor('createdAt', {
    header: 'Created',
    cell: (info) => (
      <span className="text-sm text-fm-on-surface-variant">
        {format(new Date(info.getValue()), 'MMM d, HH:mm')}
      </span>
    ),
    size: 140,
  }),
  columnHelper.display({
    id: 'actions',
    cell: (info) => (
      <Button
        asChild
        variant="ghost"
        size="icon"
        className="h-8 w-8 text-fm-on-surface-variant hover:text-fm-primary"
      >
        <Link to={`/content/${info.row.original.id}`}>
          <ExternalLink className="h-3.5 w-3.5" />
        </Link>
      </Button>
    ),
    size: 60,
  }),
]

interface ContentTableProps {
  data: ContentItemDto[]
}

export function ContentTable({ data }: ContentTableProps) {
  const [sorting, setSorting] = useState<SortingState>([])

  const table = useReactTable({
    data: useMemo(() => data, [data]),
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: { pagination: { pageSize: 25 } },
  })

  return (
    <div>
      <div className="overflow-hidden rounded-xl bg-fm-surface-container">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id} className="border-fm-outline-variant/15 hover:bg-transparent">
                {headerGroup.headers.map((header) => (
                  <TableHead
                    key={header.id}
                    className="cursor-pointer select-none text-xs font-medium uppercase tracking-wider text-fm-on-surface-variant"
                    onClick={header.column.getToggleSortingHandler()}
                  >
                    {flexRender(header.column.columnDef.header, header.getContext())}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center text-fm-on-surface-variant">
                  No content items found.
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row, rowIndex) => (
                <TableRow
                  key={row.id}
                  className={`border-fm-outline-variant/10 transition-colors hover:bg-fm-surface-container-high ${
                    rowIndex % 2 === 0 ? 'bg-fm-surface' : 'bg-fm-surface-container-low'
                  }`}
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between pt-4">
        <p className="text-sm text-fm-on-surface-variant">
          {table.getFilteredRowModel().rows.length} item(s)
        </p>
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-fm-on-surface-variant hover:bg-fm-surface-container-high hover:text-fm-on-surface"
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm text-fm-on-surface-variant">
            Page {table.getState().pagination.pageIndex + 1} of {table.getPageCount()}
          </span>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-fm-on-surface-variant hover:bg-fm-surface-container-high hover:text-fm-on-surface"
            onClick={() => table.nextPage()}
            disabled={!table.getCanNextPage()}
          >
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  )
}
