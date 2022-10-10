import {baseUrl} from "./auth";
import UpdateCategoryModel from "../model/updateCategoryModel";


const updateCategory = async (category: UpdateCategoryModel, token: string | null)=> {
    await fetch(`${baseUrl}/api/transaction/UpdateCategory`, {
        method: 'POST',
        headers: new Headers({
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
        }),
        body: JSON.stringify(category),
    });
}

export default updateCategory;