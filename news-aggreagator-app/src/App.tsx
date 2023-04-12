import React, {useEffect, useState} from 'react'
import './App.css'
import {
    CircularProgress,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TablePagination,
    TableRow
} from "@mui/material";
import {ApiClient} from "./apiClient";
import parse from 'html-react-parser';

export type ApiNews = {
    id: string
    registrationDate: string,
    sourceSite: string
    sourceName: string
    keywords: string[]
    header: string
    newsText: string
}

type Props = {
    apiClient: ApiClient
}

function App(props: Props) {

    const handleChangePage = (event: React.MouseEvent<HTMLButtonElement> | null, newPage: number) => {
        setPage(newPage);
    };
    const [page, setPage] = useState(0);
    const rowsPerPage = 10;
    const [data, setData] = useState<ApiNews[]>([]);
    const [loading, setLoading] = useState(false);
    const [keywords, setKeywords] = useState<string[]>([]);
    useEffect(() => {
        const fetchData = async () => {
            setLoading(true); // Set loading to true before fetching data
            const skip = page * rowsPerPage;
            const data = await props.apiClient.GetNews(null, rowsPerPage, skip, false, null, null)
            const keywords = await props.apiClient.GetKeywords()
            setData(data ?? []);
            setKeywords(keywords ?? [])
            setLoading(false);
        };

        fetchData();
    }, [page, rowsPerPage]);

    return (
        <div style={{display: 'flex', flexDirection: "column"}}>
            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Дата публикации</TableCell>
                            <TableCell>Сайт источника</TableCell>
                            <TableCell>Назвавние источника</TableCell>
                            <TableCell>Ключевые слова</TableCell>
                            <TableCell>Заголовок</TableCell>
                            <TableCell>Текст новости</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {loading ? (
                            <TableRow>
                                <TableCell colSpan={6} align="center">
                                    <CircularProgress/>
                                </TableCell>
                            </TableRow>
                        ) : (
                            data.map((item) => (
                                <TableRow key={item.id}>
                                    <TableCell style={{
                                        width: '10%',
                                        verticalAlign: 'top'
                                    }}>{new Date(Date.parse(item.registrationDate)).toLocaleDateString()}</TableCell>
                                    <TableCell
                                        style={{width: '10%', verticalAlign: 'top'}}>{item.sourceSite}</TableCell>
                                    <TableCell
                                        style={{width: '10%', verticalAlign: 'top'}}>{item.sourceName}</TableCell>
                                    <TableCell style={{
                                        width: '10%',
                                        verticalAlign: 'top'
                                    }}>{item.keywords?.join(', ') ?? '-'}</TableCell>
                                    <TableCell style={{width: '10%', verticalAlign: 'top'}}>{item.header}</TableCell>
                                    <TableCell style={{verticalAlign: 'top', maxWidth: '1000px', overflowX: "scroll"}}>{
                                        <div style={{
                                            whiteSpace: 'pre-line',
                                            wordBreak: 'break-word',
                                            display: 'flex',
                                            flexDirection: 'column'
                                        }}>{parse(item.newsText)}</div>}</TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </TableContainer>
            <TablePagination
                component="div"
                count={-1}
                rowsPerPage={rowsPerPage}
                page={page}
                onPageChange={handleChangePage}
            />
        </div>
    );
}

export default App
