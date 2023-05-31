import React, { useEffect, useState } from "react"
import "./App.css"
import { ApiClient } from "./apiClient";
import parse from "html-react-parser";
import { Button, DatePicker, Layout, Table, TableProps } from "antd";
import { ColumnsType } from "antd/es/table";
import { FilePdfTwoTone, PrinterTwoTone } from "@ant-design/icons";
import { Content, Footer, Header } from "antd/lib/layout/layout";
import Title from "antd/lib/typography/Title";
import Paragraph from "antd/lib/typography/Paragraph";
import "dayjs/locale/ru";
import dayjs from "dayjs";
import locale from "antd/es/date-picker/locale/ru_RU";
import kpfu_svg from "./assets/kpfu.svg"

dayjs.locale("ru")

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
                 onPrint: (id: string) => void,
                 onPdf: (id: string) => void): ColumnsType<ApiNews> {
  return [
    {
      title: "",
      dataIndex: "id",
      render: (value) =>
        <div style={{display: "flex"}}>
          <Button style={{marginRight: "4px"}} onClick={(_) => onPrint(value)}>
            <PrinterTwoTone/>
          </Button>
          <Button onClick={(_) => onPdf(value)}>
            <FilePdfTwoTone/>
          </Button>
        </div>
    },
    {
      title: "Дата публикации",
      dataIndex: "registrationDate",
      sorter: (_, __) => 0,
      sortDirections: ["ascend", "descend", "ascend"],
      defaultSortOrder: "descend",
      render: (value) => {
        const date = new Date(Date.parse(value));
        let day: any = date.getUTCDate()
        if (day < 10) {
          day = "0" + day;
        }
        let month: any = date.getUTCMonth() + 1
        if (month < 10) {
          month = `0${month}`;
        }
        return `${day}.${month}.${date.getUTCFullYear()}`;
      },
      filterDropdown: <RangePicker locale={locale}
                                   onChange={x => x ? setDts([new Date(x[0]!.toDate()), new Date(x[1]!.toDate())]) : setDts(null)}
                                   disabledDate={x => x.toDate() > new Date()}
                                   allowClear/>
    },
    {
      title: "Название источника",
      dataIndex: "sourceName",
      render: (_, data) => <a href={data.sourceSite} rel="noopener noreferrer"
                              target="_blank">{data.sourceName}</a>
    },
    {
      title: "Ключевые слова",
      dataIndex: "keywords",
      filters: keywords === null ? [] : keywords.map(x => ({text: x, value: x})),
      render: (value) => value?.join(", ")
    },
    {
      title: "Заголовок",
      dataIndex: "header",
      render: (_, data) => <a href={data.newsUrl} rel="noopener noreferrer"
                              target="_blank">{data.header}</a>
    },
    {
      title: "Текст новости",
      dataIndex: "newsText",
      width: "50%",
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
  const [order, setOrder] = useState<"ascend" | "descend">("descend");
  const [ellipses, setEllipses] = useState<string[]>([]);
  const [dts, setDts] = useState<[Date, Date] | null>(null);
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      const skip = page * rowsPerPage;
      const data = await props.apiClient.GetNews(filteredKeywords, rowsPerPage, skip, order === "ascend", dts ? dts[0] : null, dts ? dts[1] : null)

      if (keywords.length === 0) {
        const apiKeywords = await props.apiClient.GetKeywords()
        setKeywords(apiKeywords ?? [])
      }

      setData(data ?? []);
      setLoading(false);
    };
    fetchData();
  }, [page, order, filteredKeywords, dts]);
  const onChange: TableProps<ApiNews>["onChange"] = async (_, filters, sorter, __) => {
    if (sorter !== null)
      setOrder((sorter as any).order)
    if (filters !== null) {
      setFilteredKeywords(filters.keywords as string[] ?? [])
    }
  };

  const onPrint = (id: string) => {
    /* const news = data.find(x => x.id === id)!
     // почему такие параметры понятия не имею
     const winPrint = window.open('', '', 'left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0')!;
     winPrint.document.write(`<p>${new Date(Date.parse(news.registrationDate)).toLocaleDateString()}</p>`);
     winPrint.document.write(`<h1>${news.header}</h1>`);
     winPrint.document.write(news.newsText);
     winPrint.document.close();
     winPrint.focus();
     winPrint.print();
     setTimeout(()=>winPrint.close(), 200)*/
    window.open(`https://kpfu.ru/new_print?p_cid=${id}`, "_blank")?.focus()
  }

  const onPdf = (id: string) => {
    window.open(`https://kpfu.ru/pdf/portal/content/${id}.pdf`, "_blank")?.focus()
    /*const news = data.find(x => x.id === id)!
    const winPrint = window.open('', '', 'left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0')!;
    winPrint.document.write('<html><body style="/!*justify-content: left; display:flex; flex-direction: column;*!/ font-family: PTSans,serif; font-size: 8px; /!*height: 100vh;*!/ ">' + news.newsText + '</body></html>');
    winPrint.document.close();
    winPrint.focus();
    const doc = new jsPDF('p', 'pt', 'letter'/!*"a4"*!/, false);
    const margins = {
        top: 40,
        bottom: 60,
        left: 40,
        width: winPrint.innerWidth
    };
    doc.setFont('PTSans');
    //doc.setFontSize(4);
    doc.html(winPrint.document.body/!* `<div style="font-family: PTSans,serif; font-size: 8px; width: 522px">${news.newsText}</div>`*!/, {
        callback: function (doc) {
            doc.save(`${id}.pdf`);
        },
        autoPaging: "text",
        width: margins.width,
        x: /!*margins.left*!/0,
        y: /!*margins.top*!/0,
        margin: [margins.top, margins.left, margins.bottom, margins.left],
        windowWidth: winPrint.innerWidth
    })*/
    //winPrint.close();
  }

  return (
    <Layout style={{maxHeight: "100vh", height: "100vh", width: "100vw"}}>
      <Header style={{background: "#012a77", display: "flex", justifyContent: "space-between"}}>
        <div style={{display: "flex"}}>
          <div style={{marginTop: "5px"}}><img src={kpfu_svg} alt={"logo"}/></div>
          <Title style={{color: "white", marginLeft: "20px", marginTop: 5, marginBottom: 0}}>Новостной
            агрегатор</Title>
        </div>
      </Header>
      <Content><Table loading={loading}
                      style={{maxHeight: "85vh", overflow: "scroll", verticalAlign: "top"}}
                      columns={columns(keywords, ellipses, setEllipses, dts, setDts, onPrint, onPdf)}
                      dataSource={data}
                      rowKey={(n) => n.id}
                      onChange={onChange}
                      pagination={false}/></Content>

      <Footer>
        <div style={{display: "flex", justifyContent: "right"}}>
          <Button disabled={page === 0 || loading}
                  onClick={() => setPage(p => p - 1)}>{"<"}</Button>
          <div
            style={{marginLeft: "20px", marginRight: "20px", paddingTop: "2.5px"}}>{page + 1}</div>
          <Button disabled={loading} onClick={() => setPage(p => p + 1)}
                  style={{marginRight: "20px"}}>{">"}</Button>
        </div>
      </Footer>
    </Layout>

  );
}

export default App
