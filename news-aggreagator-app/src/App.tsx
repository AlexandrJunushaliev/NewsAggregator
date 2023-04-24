import React, {useEffect, useState} from 'react'
import './App.css'
import {ApiClient} from "./apiClient";
import parse from 'html-react-parser';
import {Button, Checkbox, DatePicker, Layout, Table, TableProps} from "antd";
import {ColumnsType} from "antd/es/table";
import {
    CopyTwoTone
} from '@ant-design/icons';
import {Content, Footer, Header} from "antd/lib/layout/layout";
import Title from "antd/lib/typography/Title";
import Paragraph from "antd/lib/typography/Paragraph";
import 'dayjs/locale/ru';
import dayjs from 'dayjs';
import locale from 'antd/es/date-picker/locale/ru_RU';
import kpfu_svg from './assets/kpfu.svg'

dayjs.locale('ru')

export type ApiNews = {
    id: string
    registrationDate: string,
    sourceSite: string
    sourceName: string
    keywords: string[]
    header: string
    newsText: string,
    newsUrl: string
}
const {RangePicker} = DatePicker

// это могло бы быть внутри компонента, но уже поздно
function columns(keywords: string[] | null,
                 expanded: string[],
                 setExpanded: React.Dispatch<React.SetStateAction<string[]>>,
                 dts: [Date, Date] | null,
                 setDts: React.Dispatch<React.SetStateAction<[Date, Date] | null>>,
                 ids: string[],
                 setIds: React.Dispatch<React.SetStateAction<string[]>>): ColumnsType<ApiNews> {
    return [
        {
            title: '',
            dataIndex: 'id',
            // n^2 complexity lol
            render: (value) => <Checkbox checked={!!ids.find(x => x == value)} onClick={_ => {
                const checked = !!ids.find(x => x == value)
                !checked ? setIds(prev => [...prev, value]) : setIds(prev => prev.filter(x => x !== value))
            }
            }/>
        },
        {
            title: 'Дата публикации',
            dataIndex: 'registrationDate',
            sorter: (_, __) => 0,
            sortDirections: ['ascend', 'descend', 'ascend'],
            defaultSortOrder: 'descend',
            render: (value) => new Date(Date.parse(value)).toLocaleDateString(),
            filterDropdown: <RangePicker locale={locale}
                                         onChange={x => x ? setDts([new Date(x[0]!.toDate()), new Date(x[1]!.toDate())]) : setDts(null)}
                                         allowClear/>
        },
        {
            title: 'Название источника',
            dataIndex: 'sourceName',
            render: (_, data) => <a href={data.sourceSite} rel="noopener noreferrer"
                                    target="_blank">{data.sourceName}</a>
        },
        {
            title: 'Ключевые слова',
            dataIndex: 'keywords',
            filters: keywords === null ? [] : keywords.map(x => ({text: x, value: x})),
            render: (value) => value?.join(', ')
        },
        {
            title: 'Заголовок',
            dataIndex: 'header',
            render: (_, data) => <a href={data.newsUrl} rel="noopener noreferrer" target="_blank">{data.header}</a>
        },
        {
            title: 'Текст новости',
            dataIndex: 'newsText',
            width: '50%',
            onCell: (data, rowIndex) => {
                return {
                    onClick: (_) => setExpanded(prev => {
                        if (prev.find(x => x === data.id)) {
                            return prev.filter(x => x !== data.id)
                        }
                        return [...prev, data.id]
                    }),
                };
            },
            render: (value, data) => <Paragraph
                ellipsis={!expanded.find(x => x === data.id) ? {
                    rows: 7,
                    expandable: true,
                    symbol: <></>
                } : undefined}>{parse(value)}</Paragraph>

        },
    ];
}

type Props = {
    apiClient: ApiClient
}


function App(props: Props) {
    const [page, setPage] = useState(0);
    const rowsPerPage = 10;
    const [data, setData] = useState<ApiNews[]>([]);
    const [loading, setLoading] = useState(false);
    const [keywords, setKeywords] = useState<string[]>([]);
    const [filteredKeywords, setFilteredKeywords] = useState<string[]>([]);
    const [order, setOrder] = useState<'ascend' | 'descend'>('descend');
    const [ellipses, setEllipses] = useState<string[]>([]);
    const [ids, setIds] = useState<string[]>([]);
    const [dts, setDts] = useState<[Date, Date] | null>(null);
    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            const skip = page * rowsPerPage;
            const data = await props.apiClient.GetNews(filteredKeywords, rowsPerPage, skip, order === 'ascend', dts ? dts[0] : null, dts ? dts[1] : null)
            const keywords = await props.apiClient.GetKeywords()
            setData(data ?? []);
            setKeywords(keywords ?? [])
            setLoading(false);
        };
        fetchData();
    }, [page, order, filteredKeywords, dts]);
    const onChange: TableProps<ApiNews>['onChange'] = async (_, filters, sorter, __) => {
        if (sorter !== null)
            setOrder((sorter as any).order)
        if (filters !== null) {
            setFilteredKeywords(filters.keywords as string[] ?? [])
        }

    };
    return (
        <Layout style={{maxHeight: '100vh', height: '100vh', width: '100vw'}}>
            <Header style={{background: '#012a77', display: "flex", justifyContent: "space-between"}}>
                <div style={{display: "flex"}}>
                    <div style={{marginTop:'5px'}}><img src={kpfu_svg} alt={'logo'}/></div>
                    <Title style={{color: 'white', marginLeft: '20px', marginTop: 5, marginBottom: 0}}>Новостной
                        агрегатор</Title>
                </div>
                {ids.length !== 0 ? <div>
                    <Button>buba</Button>
                </div> : <></>}
            </Header>
            <Content><Table loading={loading} style={{maxHeight: '85vh', overflow: "scroll", verticalAlign: "top"}}
                            columns={columns(keywords, ellipses, setEllipses, dts, setDts, ids, setIds)}
                            dataSource={data}
                            rowKey={(n) => n.id}
                            onChange={onChange}
                            pagination={false}/></Content>

            <Footer>
                <div style={{display: 'flex', justifyContent: 'right'}}>
                    <Button disabled={page === 0 || loading} onClick={() => setPage(p => p - 1)}>{"<"}</Button>
                    <div style={{marginLeft: '20px', marginRight: '20px', paddingTop: '2.5px'}}>{page + 1}</div>
                    <Button disabled={loading} onClick={() => setPage(p => p + 1)}
                            style={{marginRight: '20px'}}>{">"}</Button>
                </div>
            </Footer>
        </Layout>

    );
}

export default App
