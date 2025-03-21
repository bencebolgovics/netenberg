import { useState } from 'react'
import { Button } from './components/ui/button'
import { Input } from './components/ui/input.tsx'
import { Card, CardContent, CardHeader, CardTitle } from './components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './components/ui/table'
import { Skeleton } from './components/ui/skeleton'

interface Book {
    id: number
    title: string
    publisher: string
    downloads: number
}

export default function APILanding() {
    const [books, setBooks] = useState<Book[]>([])
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState('')
    const [params, setParams] = useState({
        ids: '',
        sortBy: '',
        page: 1,
        pageSize: 10
    })
    
    const searchBooks = async () => {
        setLoading(true)
        try {
            const query = new URLSearchParams({
                ...params,
                page: params.page.toString(),
                pageSize: params.pageSize.toString()
            }).toString()

            const response = await fetch(`https://localhost:7108/books?${query}`)
            if (!response.ok) throw new Error('Failed to fetch')

            const data = await response.json()
            setBooks(data.items)
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch books')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="max-w-4xl mx-auto p-6">
            <header className="mb-12 text-center">
                <h1 className="text-4xl font-bold mb-4">Gutenberg Books API</h1>
                <p className="text-muted-foreground">
                    Access Project Gutenberg book metadata with ease
                </p>
            </header>

            <Card className="mb-8">
                <CardHeader>
                    <CardTitle>API Demo</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="flex gap-4 flex-wrap">
                        <Input
                            placeholder="Filter by IDs (comma-separated)"
                            value={params.ids}
                            onChange={(e) => setParams({ ...params, ids: e.target.value })}
                        />
                        <select
                            className="border rounded-md px-4 py-2"
                            value={params.sortBy}
                            onChange={(e) => setParams({ ...params, sortBy: e.target.value })}
                        >
                            <option value="">Sort by...</option>
                            <option value="+title">Title Ascending</option>
                            <option value="-title">Title Descending</option>
                            <option value="+downloadCount">Downloads Ascending</option>
                            <option value="-downloadCount">Downloads Descending</option>
                        </select>
                        <Button onClick={searchBooks}>Search</Button>
                    </div>

                    {error && <p className="text-red-500">{error}</p>}

                    {loading ? (
                        <div className="space-y-4">
                            <Skeleton className="h-[40px] w-full" />
                            <Skeleton className="h-[40px] w-full" />
                            <Skeleton className="h-[40px] w-full" />
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>ID</TableHead>
                                    <TableHead>Title</TableHead>
                                    <TableHead>Publisher</TableHead>
                                    <TableHead>Downloads</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {books.map((book) => (
                                    <TableRow key={book.id}>
                                        <TableCell>{book.id}</TableCell>
                                        <TableCell>{book.title}</TableCell>
                                        <TableCell>{book.publisher}</TableCell>
                                        <TableCell>{book.downloads}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>

            <div className="grid md:grid-cols-2 gap-6">
                <Card>
                    <CardHeader>
                        <CardTitle>API Endpoints</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div>
                            <h3 className="font-medium mb-2">GET /books</h3>
                            <code className="text-sm bg-muted p-2 rounded">
                                ?ids=1,2,3&sortBy=+title&page=1&pageSize=10
                            </code>
                        </div>
                        <div>
                            <h3 className="font-medium mb-2">GET /books/&#123;id&#125;</h3>
                            <code className="text-sm bg-muted p-2 rounded">
                                /books/123
                            </code>
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle>Documentation</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-2">
                        <p>Parameters:</p>
                        <ul className="list-disc pl-6 text-sm text-muted-foreground">
                            <li><code>ids</code>: Comma-separated list of book IDs</li>
                            <li><code>sortBy</code>: +field or -field for sorting</li>
                            <li><code>page</code>: Pagination page number</li>
                            <li><code>pageSize</code>: Items per page</li>
                        </ul>
                    </CardContent>
                </Card>
            </div>
        </div>
    )
}